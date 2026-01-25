using EXE201.Service.DTOs.WishlistDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IWishlistService
    {
   // Get all wishlists for a specific user
        Task<IEnumerable<WishlistResponseDto>> GetWishlistsByUserIdAsync(int userId);
   
 // Get specific wishlist by ID (for authorization checks)
        Task<WishlistResponseDto?> GetByIdAsync(int id);
        
        // Add outfit to user's wishlist (returns false if already exists)
        Task<bool> AddToWishlistAsync(int userId, AddToWishlistDto dto);
        
        // Remove from wishlist
  Task<bool> RemoveFromWishlistAsync(int userId, int outfitId);
        
        // Check if outfit is already in user's wishlist
        Task<WishlistCheckDto> IsInWishlistAsync(int userId, int outfitId);
    }
}
