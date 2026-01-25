using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.OutfitImageDTOs
{
public class UpdateOutfitImageDto
    {
        [Url(ErrorMessage = "ImageUrl must be a valid URL")]
        public string? ImageUrl { get; set; }

     [MaxLength(50, ErrorMessage = "ImageType cannot exceed 50 characters")]
        public string? ImageType { get; set; }

        [Range(0, 999, ErrorMessage = "SortOrder must be between 0 and 999")]
public int? SortOrder { get; set; }
    }
}
