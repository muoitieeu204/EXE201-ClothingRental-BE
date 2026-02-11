using EXE201.Service.DTOs.ReviewImageDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewImageController : ControllerBase
    {
        private readonly IReviewImageService _reviewImageService;

        public ReviewImageController(IReviewImageService reviewImageService)
        {
            _reviewImageService = reviewImageService;
        }

        /// <summary>
        /// Get all images for a specific review
        /// </summary>
        [HttpGet("review/{reviewId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByReviewId(int reviewId)
        {
            try
            {
                if (reviewId <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid ReviewId." });

                var images = await _reviewImageService.GetByReviewIdAsync(reviewId);
                return Ok(new { success = true, data = images, count = images.Count() });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving review images." });
            }
        }

        /// <summary>
        /// Get a specific review image by its ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid ImgId." });

                var image = await _reviewImageService.GetByIdAsync(id);
                if (image == null)
                    return NotFound(new { success = false, message = "Review image not found." });

                return Ok(new { success = true, data = image });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving review image." });
            }
        }

        /// <summary>
        /// Add a new image to a review
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreateReviewImageDto dto)
        {
            try
            {
                var created = await _reviewImageService.AddAsync(dto);
                if (created == null)
                    return BadRequest(new { success = false, message = "Could not add review image. Ensure the Review exists." });

                return StatusCode(201, new { success = true, data = created });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error adding review image." });
            }
        }

        /// <summary>
        /// Update an existing review image
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewImageDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid ImgId." });

                var updated = await _reviewImageService.UpdateAsync(id, dto);
                if (updated == null)
                    return NotFound(new { success = false, message = "Review image not found or update failed." });

                return Ok(new { success = true, data = updated });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error updating review image." });
            }
        }

        /// <summary>
        /// Delete a review image
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid ImgId." });

                var result = await _reviewImageService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Review image not found." });

                return Ok(new { success = true, message = "Review image deleted successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error deleting review image." });
            }
        }
    }
}
