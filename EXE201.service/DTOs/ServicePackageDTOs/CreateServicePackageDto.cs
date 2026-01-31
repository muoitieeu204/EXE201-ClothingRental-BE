using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.ServicePackageDTOs
{
 public class CreateServicePackageDto
    {
    [Required(ErrorMessage = "StudioId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "StudioId must be greater than 0")]
        public int StudioId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
        public string Name { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

  [Required(ErrorMessage = "BasePrice is required")]
        [Range(0, double.MaxValue, ErrorMessage = "BasePrice must be non-negative")]
        public decimal BasePrice { get; set; }
    }
}
