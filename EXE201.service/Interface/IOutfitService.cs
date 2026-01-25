using EXE201.Service.DTOs.OutfitDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IOutfitService
    {
        /// <summary>
        /// Get all outfits with basic information
        /// </summary>
      Task<IEnumerable<OutfitResponseDto>> GetAllAsync();
        
   /// <summary>
 /// Get available outfits (status = 'Available')
        /// </summary>
        Task<IEnumerable<OutfitResponseDto>> GetAvailableOutfitsAsync();
        
        /// <summary>
  /// Get outfits by category
        /// </summary>
        Task<IEnumerable<OutfitResponseDto>> GetOutfitsByCategoryIdAsync(int categoryId);
      
   /// <summary>
   /// Get a specific outfit by ID with basic info
        /// </summary>
        Task<OutfitResponseDto?> GetByIdAsync(int outfitId);
        
/// <summary>
        /// Get detailed outfit information including images, sizes, reviews
   /// </summary>
   Task<OutfitDetailDto?> GetDetailByIdAsync(int outfitId);
        
        /// <summary>
    /// Search outfits by name, type, or other criteria
   /// </summary>
   Task<IEnumerable<OutfitResponseDto>> SearchAsync(string searchTerm);
 
        /// <summary>
    /// Create a new outfit
/// </summary>
        Task<int> AddAsync(CreateOutfitDto dto);
    
        /// <summary>
        /// Update an existing outfit
        /// </summary>
        Task<bool> UpdateAsync(int outfitId, UpdateOutfitDto dto);
        
        /// <summary>
    /// Delete an outfit
        /// </summary>
   Task<bool> DeleteAsync(int outfitId);
    }
}
