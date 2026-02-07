using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.OutfitDTOs
{
    public class CreateOutfitDto
    {
      [Required(ErrorMessage = "CategoryId is required")]
[Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Name is required")]
     [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
        public string? Type { get; set; }

        [StringLength(50, ErrorMessage = "Gender cannot exceed 50 characters")]
        public string? Gender { get; set; }

        [StringLength(100, ErrorMessage = "Region cannot exceed 100 characters")]
        public string? Region { get; set; }

        public bool? IsLimited { get; set; }

  [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }

        [Required(ErrorMessage = "BaseRentalPrice is required")]
        [Range(0, double.MaxValue, ErrorMessage = "BaseRentalPrice must be non-negative")]
     public decimal BaseRentalPrice { get; set; }
    }
}
