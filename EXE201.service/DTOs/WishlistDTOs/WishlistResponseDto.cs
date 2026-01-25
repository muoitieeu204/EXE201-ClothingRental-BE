using System;

namespace EXE201.Service.DTOs.WishlistDTOs
{
    public class WishlistResponseDto
    {
        public int WishlistId { get; set; }
        public int UserId { get; set; }
        public int OutfitId { get; set; }
  public DateTime? AddedAt { get; set; }
        
        // Optional: Include outfit details
    public string? OutfitName { get; set; }
public decimal? OutfitPrice { get; set; }
      public string? OutfitImageUrl { get; set; }
    }
}
