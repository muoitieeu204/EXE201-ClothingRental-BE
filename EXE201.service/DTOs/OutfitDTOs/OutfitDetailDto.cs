using System;
using System.Collections.Generic;

namespace EXE201.Service.DTOs.OutfitDTOs
{
/// <summary>
    /// Detailed outfit response with full related data
    /// </summary>
    public class OutfitDetailDto
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
    
   // Category info
    public string? CategoryName { get; set; }
        
  // Images
        public List<OutfitImageInfo> Images { get; set; } = new();
        
   // Sizes
  public List<OutfitSizeInfo> Sizes { get; set; } = new();
    
  // Reviews summary
     public double? AverageRating { get; set; }
 public int TotalReviews { get; set; }
 
  // Attributes
        public OutfitAttributeInfo? Attributes { get; set; }
    }

    public class OutfitImageInfo
    {
        public int ImageId { get; set; }
   public string ImageUrl { get; set; } = null!;
   public string? ImageType { get; set; }
    public int? SortOrder { get; set; }
    }

    public class OutfitSizeInfo
{
      public int SizeId { get; set; }
        public string SizeLabel { get; set; } = null!;
 public int? StockQuantity { get; set; }
 public string? Status { get; set; }
    }

    public class OutfitAttributeInfo
    {
      public int DetailId { get; set; }
        public string? Material { get; set; }
 public string? Silhouette { get; set; }
     public string? FormalityLevel { get; set; }
        public string? Occasion { get; set; }
        public string? ColorPrimary { get; set; }
  public string? SeasonSuitability { get; set; }
 public string? StoryTitle { get; set; }
     public string? StoryContent { get; set; }
     public string? CulturalOrigin { get; set; }
    }
}
