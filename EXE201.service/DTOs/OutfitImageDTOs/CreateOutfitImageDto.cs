using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.OutfitImageDTOs
{
    public class CreateOutfitImageDto
    {
        [Required(ErrorMessage = "OutfitId is required")]
   [Range(1, int.MaxValue, ErrorMessage = "OutfitId must be greater than 0")]
     public int OutfitId { get; set; }

        [Required(ErrorMessage = "ImageUrl is required")]
     [Url(ErrorMessage = "ImageUrl must be a valid URL")]
        public string ImageUrl { get; set; } = null!;

        [MaxLength(50, ErrorMessage = "ImageType cannot exceed 50 characters")]
        public string? ImageType { get; set; }

        [Range(0, 999, ErrorMessage = "SortOrder must be between 0 and 999")]
        public int? SortOrder { get; set; }
}
}
