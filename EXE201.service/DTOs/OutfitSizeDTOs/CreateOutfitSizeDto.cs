using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.OutfitSizeDTOs
{
    public class CreateOutfitSizeDto
    {
        [Required(ErrorMessage = "OutfitId is required")]
     [Range(1, int.MaxValue, ErrorMessage = "OutfitId must be greater than 0")]
    public int OutfitId { get; set; }

        [Required(ErrorMessage = "SizeLabel is required")]
  [StringLength(50, ErrorMessage = "SizeLabel cannot exceed 50 characters")]
        public string SizeLabel { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "StockQuantity must be non-negative")]
        public int? StockQuantity { get; set; }

        [Range(0, 300, ErrorMessage = "ChestMaxCm must be between 0 and 300")]
    public double? ChestMaxCm { get; set; }

        [Range(0, 300, ErrorMessage = "WaistMaxCm must be between 0 and 300")]
        public double? WaistMaxCm { get; set; }

     [Range(0, 300, ErrorMessage = "HipMaxCm must be between 0 and 300")]
        public double? HipMaxCm { get; set; }

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }
    }
}
