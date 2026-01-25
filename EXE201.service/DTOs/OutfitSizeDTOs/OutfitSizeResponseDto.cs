namespace EXE201.Service.DTOs.OutfitSizeDTOs
{
    public class OutfitSizeResponseDto
    {
        public int SizeId { get; set; }
    public int OutfitId { get; set; }
    public string SizeLabel { get; set; } = null!;
        public int? StockQuantity { get; set; }
     public double? ChestMaxCm { get; set; }
        public double? WaistMaxCm { get; set; }
     public double? HipMaxCm { get; set; }
 public string? Status { get; set; }
        
        // Optional: Include outfit name for reference
        public string? OutfitName { get; set; }
    }
}
