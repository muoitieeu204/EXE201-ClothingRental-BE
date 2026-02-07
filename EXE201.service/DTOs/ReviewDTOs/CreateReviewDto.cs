using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.ReviewDTOs
{
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "OutfitId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "OutfitId must be greater than 0")]
        public int OutfitId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }

        public int? Rating { get; set; }
        public string? Comment { get; set; }

        public List<string>? ImageUrls { get; set; }
    }
}
