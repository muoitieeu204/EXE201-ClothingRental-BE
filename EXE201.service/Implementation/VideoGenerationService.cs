using EXE201.Repository.Interfaces;
using EXE201.Service.DTOs.GeminiDTOs;
using EXE201.Service.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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

            // Get outfit images from database (these are URLs that work for frontend)
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

            // Convert URL to base64 for Gemini API
            // Frontend uses URL directly, but Gemini API needs base64
            string? imageBase64 = null;
            string mimeType = "image/jpeg"; // default
            try
            {
                var firstImage = outfitImages.OrderBy(i => i.SortOrder ?? int.MaxValue).First();
                // Download and convert: URL → Bytes → Base64 + detect MIME type
                var imageData = await DownloadImageWithMimeTypeAsync(firstImage.ImageUrl);
                imageBase64 = imageData.Base64String;
                mimeType = imageData.MimeType;
            }
            catch (Exception ex)
            {
                return new VideoGenerationResponse
                {
                    Status = "failed",
                    Message = $"Failed to download and convert image: {ex.Message}"
                };
            }

            // Create Gemini API request - Veo model format
            // Gemini requires base64 + MIME type, not URLs
            var geminiRequest = new
            {
                instances = new[]
               {
                   new
                   {
                       prompt = request.Prompt,
                       referenceImages = new[]
                       {
                           new
                           {
                               referenceType= "asset",
                               image = new
                               {
                                   mimeType = mimeType, // e.g., "image/jpeg"
                                   bytesBase64Encoded = imageBase64 // converted base64 string
                               }
                           }
                       }
                   }
               },
               parameters = new
               {
                   sampleCount = 1,
                   aspectRatio = "16:9",
                   resolution = "720p"
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

                // 1. Get the raw URI from the response
                var rawUri = operationResponse.Response?.GenerateVideoResponse?.GeneratedSamples?[0]?.Video?.Uri;

                // 2. FIX: Append the API key so the URL is authorized for viewing/downloading
                // Without this, the frontend or your DownloadVideoAsync method will hit a 403 error.
                var authenticatedVideoUri = string.IsNullOrEmpty(rawUri)
                    ? null
                    : $"{rawUri}&key={_geminiApiKey}";

                return new VideoStatusResponse
                {
                    IsComplete = true,
                    Status = "completed",
                    VideoUri = authenticatedVideoUri
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

        /// <summary>
        /// Downloads image from URL and converts to base64 with MIME type detection.
        /// This is needed because Gemini API doesn't accept direct URLs like frontend does.
        /// </summary>
        private async Task<ImageData> DownloadImageWithMimeTypeAsync(string imageUrl)
        {
            // Handle data URI format (data:image/jpeg;base64,...)
            // Some systems store base64 directly in database
            if (imageUrl.StartsWith("data:"))
            {
                var parts = imageUrl.Split(',');
                if (parts.Length == 2)
                {
                    var mimeTypePart = parts[0].Split(':')[1].Split(';')[0];
                    ValidateMimeType(mimeTypePart);
                    return new ImageData(parts[1], mimeTypePart);
                }
                throw new InvalidOperationException("Invalid data URI format");
            }

            // Download from URL (your current database format)
            var response = await _httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var base64String = Convert.ToBase64String(imageBytes);
            
            // Detect MIME type from HTTP response or file extension
            var mimeType = response.Content.Headers.ContentType?.MediaType 
                           ?? DetectMimeTypeFromUrl(imageUrl);
            
            ValidateMimeType(mimeType);
            
            return new ImageData(base64String, mimeType);
        }

        /// <summary>
        /// Detects MIME type from file extension
        /// </summary>
        private string DetectMimeTypeFromUrl(string imageUrl)
        {
            var extension = Path.GetExtension(imageUrl).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg" // default fallback
            };
        }

        /// <summary>
        /// Validates that MIME type is supported by Gemini API
        /// </summary>
        private void ValidateMimeType(string mimeType)
        {
            var supportedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!supportedTypes.Contains(mimeType.ToLowerInvariant()))
            {
                throw new InvalidOperationException(
                    $"Unsupported image type: {mimeType}. Supported types: {string.Join(", ", supportedTypes)}");
            }
        }

        /// <summary>
        /// Data container for base64 string and its MIME type
        /// </summary>
        private record ImageData(string Base64String, string MimeType);
    }
}
