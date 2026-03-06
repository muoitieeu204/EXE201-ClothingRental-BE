using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.GeminiDTOs
{
    public class VideoGenerationRequest
    {
        [Required(ErrorMessage = "OutfitId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "OutfitId must be greater than 0")]
        public int OutfitId { get; set; }

        [Required(ErrorMessage = "Prompt is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Prompt must be between 10 and 2000 characters")]
        public string Prompt { get; set; } = null!;

        /// <summary>
        /// Optional: Specific image IDs to use. If null, all outfit images will be used.
        /// </summary>
        public List<int>? ImageIds { get; set; }
    }

    public class VideoGenerationResponse
    {
        public string? OperationName { get; set; }
        public string Status { get; set; } = "processing";
        public string? Message { get; set; }
    }

    public class VideoStatusResponse
    {
        public bool IsComplete { get; set; }
        public string Status { get; set; } = null!;
        public string? VideoUri { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class GeminiVideoRequest
    {
        public List<GeminiInstance> Instances { get; set; } = new();
        public GeminiParameters Parameters { get; set; } = new();
    }

    public class GeminiInstance
    {
        public string Prompt { get; set; } = null!;
    }

    public class GeminiParameters
    {
        public List<GeminiReferenceImage> ReferenceImages { get; set; } = new();
    }

    public class GeminiReferenceImage
    {
        public GeminiImage Image { get; set; } = new();
        public string ReferenceType { get; set; } = "asset";
    }

    public class GeminiImage
    {
        public GeminiInlineData InlineData { get; set; } = new();
    }

    public class GeminiInlineData
    {
        public string MimeType { get; set; } = "image/png";
        public string Data { get; set; } = null!;
    }

    public class GeminiOperationResponse
    {
        public string? Name { get; set; }
        public bool Done { get; set; }
        public GeminiVideoResponse? Response { get; set; }
        public GeminiError? Error { get; set; }
    }

    public class GeminiVideoResponse
    {
        public GeminiGenerateVideoResponse? GenerateVideoResponse { get; set; }
    }

    public class GeminiGenerateVideoResponse
    {
        public List<GeminiGeneratedSample>? GeneratedSamples { get; set; }
    }

    public class GeminiGeneratedSample
    {
        public GeminiVideo? Video { get; set; }
    }

    public class GeminiVideo
    {
        public string? Uri { get; set; }
    }

    public class GeminiError
    {
        public int Code { get; set; }
        public string? Message { get; set; }
    }
}
