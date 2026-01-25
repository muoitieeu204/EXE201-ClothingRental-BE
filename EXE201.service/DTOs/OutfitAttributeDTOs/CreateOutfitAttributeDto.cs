using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.OutfitAttributeDTOs
{
    public class CreateOutfitAttributeDto
    {
        [Required]
        public int OutfitId { get; set; }

        public string? Material { get; set; }
        public string? Silhouette { get; set; }
        public string? FormalityLevel { get; set; }
        public string? Occasion { get; set; }
        public string? ColorPrimary { get; set; }
        public string? SeasonSuitability { get; set; }
        public string? StoryTitle { get; set; }
        public string? StoryContent { get; set; }
        public string? CulturalOrigin { get; set; }
    }
}
