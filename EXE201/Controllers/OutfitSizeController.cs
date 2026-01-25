using EXE201.Service.DTOs.OutfitSizeDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutfitSizeController : ControllerBase
    {
        private readonly IOutfitSizeService _outfitSizeService;

        public OutfitSizeController(IOutfitSizeService outfitSizeService)
        {
            _outfitSizeService = outfitSizeService;
        }

        /// <summary>
        /// Get all sizes for a specific outfit
        /// </summary>
        [HttpGet("outfit/{outfitId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSizesByOutfitId(int outfitId)
        {
            try
            {
                if (outfitId <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

                var sizes = await _outfitSizeService.GetSizesByOutfitIdAsync(outfitId);
                return Ok(new { success = true, data = sizes, count = sizes.Count() });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving outfit sizes." });
            }
        }

        /// <summary>
        /// Get available sizes (in stock) for a specific outfit
        /// </summary>
        [HttpGet("outfit/{outfitId}/available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSizesByOutfitId(int outfitId)
        {
            try
            {
                if (outfitId <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

                var sizes = await _outfitSizeService.GetAvailableSizesByOutfitIdAsync(outfitId);
                return Ok(new { success = true, data = sizes, count = sizes.Count() });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving available sizes." });
            }
        }

        /// <summary>
        /// Get a specific size by its ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid SizeId." });

                var size = await _outfitSizeService.GetByIdAsync(id);
                if (size == null)
                    return NotFound(new { success = false, message = "Outfit size not found." });

                return Ok(new { success = true, data = size });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving outfit size." });
            }
        }

        /// <summary>
        /// Add a new size to an outfit
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreateOutfitSizeDto dto)
        {
            try
            {
                var result = await _outfitSizeService.AddAsync(dto);
                if (!result)
                    return BadRequest(new { success = false, message = "Could not add size. Ensure the Outfit exists." });

                return StatusCode(201, new { success = true, message = "Outfit size added successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error adding outfit size." });
            }
        }

        /// <summary>
        /// Update an existing outfit size
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOutfitSizeDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid SizeId." });

                var result = await _outfitSizeService.UpdateAsync(id, dto);
                if (!result)
                    return NotFound(new { success = false, message = "Outfit size not found or update failed." });

                return Ok(new { success = true, message = "Outfit size updated successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error updating outfit size." });
            }
        }

        /// <summary>
        /// Delete an outfit size
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid SizeId." });

                var result = await _outfitSizeService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Outfit size not found." });

                return Ok(new { success = true, message = "Outfit size deleted successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Error deleting outfit size." });
            }
        }
    }
}
