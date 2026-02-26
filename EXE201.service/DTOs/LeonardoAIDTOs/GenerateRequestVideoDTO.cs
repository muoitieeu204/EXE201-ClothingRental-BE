using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.LeonardoAIDTOs
{
    public class GenerateRequestVideoDTO
    {
        [Required(ErrorMessage = "Prompt is required")]
        public string Prompt { get; set; } = string.Empty;
        public int Duration { get; set; } = 4;
        public string? Mode { get; set; } = "RESOLUTION_720";
        public int? Width { get; set; } = 1248;
        public int? Height { get; set; } = 704;
        public string? PromptEnhance { get; set; } = "OFF";
        public bool Public { get; set; } = false;
        public SeedanceGuidances Guidances { get; set; } = new();

        public class SeedanceGuidances
        {
            public List<SeedanceFrameItem>? StartFrame { get; set; }
            public List<SeedanceFrameItem>? EndFrame { get; set; }
        }

        public class SeedanceFrameItem
        {
            public SeedanceImageReference Image { get; set; } = new();
        }

        public class SeedanceImageReference
        {
            public string Id { get; set; } = string.Empty;
            public string Type { get; set; } = "UPLOADED";
        }
    }
}
