using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.ReviewImageDTOs
{
    public class CreateReviewImageDto
    {
        [Required(ErrorMessage = "ReviewId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ReviewId must be greater than 0")]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "ImageUrl is required")]
        [Url(ErrorMessage = "ImageUrl must be a valid URL")]
        public string ImageUrl { get; set; } = null!;
    }
}
