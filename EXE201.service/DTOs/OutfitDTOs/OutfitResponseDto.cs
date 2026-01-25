using System;

namespace EXE201.Service.DTOs.OutfitDTOs
{
    public class OutfitResponseDto
    {
        public int OutfitId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
  public string? Type { get; set; }
        public string? Gender { get; set; }
  public string? Region { get; set; }
        public bool? IsLimited { get; set; }
        public string? Status { get; set; }
        public decimal BaseRentalPrice { get; set; }
     public DateTime? CreatedAt { get; set; }
        
// Navigation properties
      public string? CategoryName { get; set; }
        
  // Aggregated data
    public int TotalImages { get; set; }
     public int TotalSizes { get; set; }
    public int AvailableSizes { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}
