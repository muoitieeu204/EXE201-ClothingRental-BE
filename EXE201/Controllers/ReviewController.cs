using EXE201.Service.DTOs.ReviewDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Get reviews (optional: reviewId, outfitId, userId)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromQuery] int? reviewId, [FromQuery] int? outfitId, [FromQuery] int? userId)
        {
            try
            {
                if (reviewId.HasValue && reviewId <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid ReviewId." });

                if (outfitId.HasValue && outfitId <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

                if (userId.HasValue && userId <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid UserId." });

                if (reviewId.HasValue)
                {
                    var review = await _reviewService.GetByIdAsync(reviewId.Value);
                    if (review == null)
                        return NotFound(new { success = false, message = "Review not found." });

                    return Ok(new { success = true, data = review });
                }

                var reviews = await _reviewService.GetAllAsync(outfitId, userId);
                return Ok(new { success = true, data = reviews, count = reviews.Count() });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving reviews." });
            }
        }

        /// <summary>
        /// Get all reviews for a specific outfit
        /// </summary>
        [HttpGet("outfit/{outfitId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByOutfitId(int outfitId)
        {
            try
            {
                if (outfitId <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

                var reviews = await _reviewService.GetAllAsync(outfitId, null);
                return Ok(new { success = true, data = reviews, count = reviews.Count() });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving reviews by outfit." });
            }
        }

        /// <summary>
        /// Create a new review
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            try
            {
                var created = await _reviewService.AddAsync(dto);
                if (created == null)
                    return BadRequest(new { success = false, message = "Could not create review. Ensure the Outfit and User exist." });

                return StatusCode(201, new { success = true, data = created });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error creating review." });
            }
        }

        /// <summary>
        /// Update an existing review
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid ReviewId." });

                var updated = await _reviewService.UpdateAsync(id, dto);
                if (updated == null)
                    return NotFound(new { success = false, message = "Review not found or update failed." });

                return Ok(new { success = true, data = updated });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error updating review." });
            }
        }

        /// <summary>
        /// Delete a review
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid ReviewId." });

                var result = await _reviewService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Review not found." });

                return Ok(new { success = true, message = "Review deleted successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error deleting review." });
            }
        }
    }
}
