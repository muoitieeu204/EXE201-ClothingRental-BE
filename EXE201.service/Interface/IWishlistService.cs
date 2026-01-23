using EXE201.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
  public interface IWishlistService
    {
        // Get all wishlists for a specific user
        Task<IEnumerable<WishlistDTO>> GetWishlistsByUserIdAsync(int userId);
   
        // Get specific wishlist by ID (for authorization checks)
    Task<WishlistDTO> GetByIdAsync(int id);
        
        // Add outfit to user's wishlist (returns false if already exists)
        Task<bool> AddToWishlistAsync(int userId, int outfitId);
        
        // Remove from wishlist
 Task<bool> RemoveFromWishlistAsync(int wishlistId, int userId);
        
      // Check if outfit is already in user's wishlist
    Task<bool> IsInWishlistAsync(int userId, int outfitId);
    }
}
