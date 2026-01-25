using EXE201.Service.DTOs;
using EXE201.Service.DTOs.WishlistDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        /// <summary>
        /// Get current user's wishlist
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyWishlist()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return Unauthorized("Invalid user token.");

                var wishlists = await _wishlistService.GetWishlistsByUserIdAsync(userId);
                return Ok(new
                {
                    success = true,
                    data = wishlists,
                    count = wishlists.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving your wishlist." });
            }
        }

        /// <summary>
        /// Check if outfit is in current user's wishlist
        /// </summary>
        [HttpGet("check/{outfitId}")]
        public async Task<IActionResult> CheckInWishlist(int outfitId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return Unauthorized("Invalid user token.");

                var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, outfitId);
                return Ok(new { isInWishlist });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred." });
            }
        }

        /// <summary>
        /// Add outfit to current user's wishlist
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistDto request)
        {
            if (request.OutfitId <= 0)
                return BadRequest(new { success = false, message = "Invalid OutfitId." });

            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return Unauthorized("Invalid user token.");

                var result = await _wishlistService.AddToWishlistAsync(userId, request);

                if (!result)
                    return Conflict(new { success = false, message = "Outfit is already in your wishlist." });

                return Ok(new { success = true, message = "Outfit added to wishlist successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding to wishlist." });
            }
        }

        /// <summary>
        /// Remove outfit from current user's wishlist
        /// </summary>
        [HttpDelete("{wishlistId}")]
        public async Task<IActionResult> RemoveFromWishlist(int wishlistId)
        {
            if (wishlistId <= 0)
                return BadRequest(new { success = false, message = "Invalid wishlist ID." });

            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return Unauthorized("Invalid user token.");

                var result = await _wishlistService.RemoveFromWishlistAsync(wishlistId, userId);

                if (!result)
                    return NotFound(new { success = false, message = "Wishlist item not found or you don't have permission." });

                return Ok(new { success = true, message = "Outfit removed from wishlist successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while removing from wishlist." });
            }
        }

        /// <summary>
        /// Alternative: Remove by outfit ID (more intuitive for frontend)
        /// </summary>
        [HttpDelete("outfit/{outfitId}")]
        public async Task<IActionResult> RemoveByOutfitId(int outfitId)
        {
            if (outfitId <= 0)
                return BadRequest(new { success = false, message = "Invalid outfit ID." });

            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0) return Unauthorized("Invalid user token.");

                // Find the wishlist item
                var wishlists = await _wishlistService.GetWishlistsByUserIdAsync(userId);
                var wishlistItem = wishlists.FirstOrDefault(w => w.OutfitId == outfitId);

                if (wishlistItem == null)
                    return NotFound(new { success = false, message = "Outfit not found in your wishlist." });

                var result = await _wishlistService.RemoveFromWishlistAsync(userId, outfitId);

                if (!result)
                    return NotFound(new { success = false, message = "Failed to remove outfit from wishlist." });

                return Ok(new { success = true, message = "Outfit removed from wishlist successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while removing from wishlist." });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
