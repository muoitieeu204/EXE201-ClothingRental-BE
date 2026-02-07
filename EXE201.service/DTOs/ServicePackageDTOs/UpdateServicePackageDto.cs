using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.ServicePackageDTOs
{
    public class UpdateServicePackageDto
    {
   [Range(1, int.MaxValue, ErrorMessage = "StudioId must be greater than 0")]
        public int? StudioId { get; set; }

        [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters")]
  public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

  [Range(0, double.MaxValue, ErrorMessage = "BasePrice must be non-negative")]
        public decimal? BasePrice { get; set; }
    }
}
