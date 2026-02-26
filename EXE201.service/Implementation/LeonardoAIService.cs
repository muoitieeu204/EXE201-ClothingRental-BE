using Azure.Core;
using EXE201.Service.DTOs.LeonardoAIDTOs;
using EXE201.Service.Interface;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace EXE201.Service.Implementation
{
    public class LeonardoAIService : ILeonardoAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public LeonardoAIService(IHttpClientFactory httpClientFactory, IConfiguration configuration, string apiKey, string baseUrl)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiKey = _configuration["LeonardoAI:ApiKey"] ?? throw new ArgumentNullException("LeonardoAI:ApiKey");
            _baseUrl = _configuration["LeonardoAI:BaseUrl"] ?? "https://cloud.leonardo.ai/api/rest";
        }

        public async Task<string?> GenerateVideoAsync(GenerateRequestVideoDTO request)
        {
            try
            {
                var client = CreateHttpClient();

                // Build payload
                var payload = new
                {
                    model = _configuration["LeonardoAI:DefaultModel"] ?? "seedance-1.0-pro",
                    @public = request.Public,
                    parameters = new
                    {
                        prompt = request.Prompt,
                        guidances = new
                        {
                            start_frame = request.Guidances.StartFrame?.Select(f => new
                            {
                                image = new { id = f.Image.Id, type = f.Image.Type }
                            }).ToArray(),
                            end_frame = request.Guidances.EndFrame?.Select(f => new
                            {
                                image = new { id = f.Image.Id, type = f.Image.Type }
                            }).ToArray()
                        },
                        duration = request.Duration,
                        mode = request.Mode,
                        prompt_enhance = request.PromptEnhance,
                        width = request.Width,
                        height = request.Height
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_baseUrl}/v2/generations", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Leonardo API error: {response.StatusCode} - {error}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                // Extract generationId directly - no intermediate DTO
                string? generationId = null;
                if (root.TryGetProperty("sdGenerationJob", out var jobElement) &&
                    jobElement.TryGetProperty("generationId", out var genIdElement))
                {
                    generationId = genIdElement.GetString();
                }

                return generationId;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error generating video: {ex.Message}", ex);
            }
        }

        public async Task<(bool isComplete, string? videoUrl, string status)> GetVideoStatusAsync(string generationId)
        {
            try
            {
                var client = CreateHttpClient();
                var response = await client.GetAsync($"{_baseUrl}/v1/generations/{generationId}");

                if (!response.IsSuccessStatusCode)
                    return (false, null, "ERROR");

                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("generations_by_pk", out var genElement))
                    return (false, null, "NOT_FOUND");

                var status = genElement.TryGetProperty("status", out var statusElement)
                    ? statusElement.GetString() ?? "UNKNOWN"
                    : "UNKNOWN";

                string? videoUrl = null;
                if (genElement.TryGetProperty("generated_videos", out var videosElement) &&
                    videosElement.ValueKind == JsonValueKind.Array &&
                    videosElement.GetArrayLength() > 0)
                {
                    var firstVideo = videosElement[0];
                    if (firstVideo.TryGetProperty("url", out var urlElement))
                    {
                        videoUrl = urlElement.GetString();
                    }
                }

                var isComplete = status == "COMPLETE" && !string.IsNullOrEmpty(videoUrl);
                return (isComplete, videoUrl, status);
            }
            catch
            {
                return (false, null, "ERROR");
            }
        }

        public async Task<string?> WaitForVideoCompletionAsync(string generationId, int maxWaitSeconds)
        {
            var startTime = DateTime.UtcNow;
            var checkInterval = TimeSpan.FromSeconds(10);

            while ((DateTime.UtcNow - startTime).TotalSeconds < maxWaitSeconds)
            {
                var (isComplete, videoUrl, status) = await GetVideoStatusAsync(generationId);

                if (isComplete && !string.IsNullOrEmpty(videoUrl))
                    return videoUrl;

                if (status == "FAILED")
                    return null;

                await Task.Delay(checkInterval);
            }

            return null; // Timeout
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            return client;
        }
    }
}
