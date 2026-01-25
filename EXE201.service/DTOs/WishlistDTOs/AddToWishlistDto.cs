using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.WishlistDTOs
{
    public class AddToWishlistDto
    {
        [Required(ErrorMessage = "OutfitId is required")]
      [Range(1, int.MaxValue, ErrorMessage = "OutfitId must be greater than 0")]
        public int OutfitId { get; set; }
    }
}
