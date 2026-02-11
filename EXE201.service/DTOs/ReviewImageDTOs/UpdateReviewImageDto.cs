using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.ReviewImageDTOs
{
    public class UpdateReviewImageDto
    {
        [Required(ErrorMessage = "ImageUrl is required")]
        [Url(ErrorMessage = "ImageUrl must be a valid URL")]
        public string ImageUrl { get; set; } = null!;
    }
}
