using EXE201.Repository.Interfaces;
using EXE201.Service.DTOs.GeminiDTOs;
using EXE201.Service.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class VideoGenerationService : IVideoGenerationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly string _baseUrl;

        public VideoGenerationService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _geminiApiKey = _configuration["GeminiAPI:ApiKey"]
                ?? throw new InvalidOperationException("GeminiAPI:ApiKey not configured");
            _baseUrl = "https://generativelanguage.googleapis.com/v1beta";
        }

        public async Task<VideoGenerationResponse> GenerateVideoAsync(VideoGenerationRequest request)
        {
            // Validate outfit exists
            var outfitExists = await _unitOfWork.Outfits.ExistAsync(o => o.OutfitId == request.OutfitId);
            if (!outfitExists)
            {
                return new VideoGenerationResponse
                {
                    Status = "failed",
                    Message = "Outfit not found"
                };
            }

            // Get outfit images
            var outfitImages = await _unitOfWork.OutfitImages.GetImagesByOutfitIdAsync(request.OutfitId);

            if (!outfitImages.Any())
            {
                return new VideoGenerationResponse
                {
                    Status = "failed",
                    Message = "No images found for this outfit"
                };
            }

            // Filter images if specific IDs provided
            if (request.ImageIds != null && request.ImageIds.Any())
            {
                outfitImages = outfitImages.Where(img => request.ImageIds.Contains(img.ImageId)).ToList();
            }

            if (!outfitImages.Any())
            {
                return new VideoGenerationResponse
                {
                    Status = "failed",
                    Message = "No matching images found"
                };
            }

            // Download and convert first image to base64
            string? imageBase64 = null;
            try
            {
                var firstImage = outfitImages.OrderBy(i => i.SortOrder ?? int.MaxValue).First();
                imageBase64 = await DownloadImageAsBase64Async(firstImage.ImageUrl);
            }
            catch (Exception ex)
            {
                return new VideoGenerationResponse
                {
                    Status = "failed",
                    Message = $"Failed to download image: {ex.Message}"
                };
            }

            // Create Gemini API request - Veo model format
            var geminiRequest = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = request.Prompt },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = "image/jpeg",
                                    data = imageBase64
                                }
                            }
                        }
                    }
                }
            };

            // Send request to Gemini API
            var url = $"{_baseUrl}/models/veo-3.1-generate-preview:predictLongRunning?key={_geminiApiKey}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(geminiRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new VideoGenerationResponse
                {
                    Status = "failed",
                    Message = $"Gemini API error: {errorContent}"
                };
            }

            var operationResponse = await response.Content.ReadFromJsonAsync<GeminiOperationResponse>();

            return new VideoGenerationResponse
            {
                OperationName = operationResponse?.Name,
                Status = "processing",
                Message = "Video generation started successfully"
            };
        }

        public async Task<VideoStatusResponse> GetVideoStatusAsync(string operationName)
        {
            var url = $"{_baseUrl}/{operationName}?key={_geminiApiKey}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new VideoStatusResponse
                {
                    IsComplete = false,
                    Status = "error",
                    ErrorMessage = $"Failed to check operation status: {errorContent}"
                };
            }

            var operationResponse = await response.Content.ReadFromJsonAsync<GeminiOperationResponse>();

            if (operationResponse?.Done == true)
            {
                if (operationResponse.Error != null)
                {
                    return new VideoStatusResponse
                    {
                        IsComplete = true,
                        Status = "failed",
                        ErrorMessage = operationResponse.Error.Message
                    };
                }

                var videoUri = operationResponse.Response?.GenerateVideoResponse?.GeneratedSamples?[0]?.Video?.Uri;

                return new VideoStatusResponse
                {
                    IsComplete = true,
                    Status = "completed",
                    VideoUri = videoUri
                };
            }

            return new VideoStatusResponse
            {
                IsComplete = false,
                Status = "processing"
            };
        }

        public async Task<byte[]> DownloadVideoAsync(string videoUri)
        {
            var url = $"{videoUri}?key={_geminiApiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        private async Task<string> DownloadImageAsBase64Async(string imageUrl)
        {
            var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            return Convert.ToBase64String(imageBytes);
        }
    }
}
