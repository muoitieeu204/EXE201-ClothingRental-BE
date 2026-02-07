using EXE201.Service.DTOs.OutfitSizeDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IOutfitSizeService
    {
        /// <summary>
        /// Get all sizes for a specific outfit
        /// </summary>
 Task<IEnumerable<OutfitSizeResponseDto>> GetSizesByOutfitIdAsync(int outfitId);
        
        /// <summary>
    /// Get only available sizes (in stock) for a specific outfit
     /// </summary>
        Task<IEnumerable<OutfitSizeResponseDto>> GetAvailableSizesByOutfitIdAsync(int outfitId);
        
   /// <summary>
        /// Get a specific size by its ID
        /// </summary>
        Task<OutfitSizeResponseDto?> GetByIdAsync(int sizeId);
        
        /// <summary>
        /// Add a new size to an outfit
        /// </summary>
      Task<bool> AddAsync(CreateOutfitSizeDto dto);
    
  /// <summary>
        /// Update an existing outfit size
        /// </summary>
    Task<bool> UpdateAsync(int sizeId, UpdateOutfitSizeDto dto);
        
  /// <summary>
        /// Delete an outfit size
    /// </summary>
        Task<bool> DeleteAsync(int sizeId);
  }
}
