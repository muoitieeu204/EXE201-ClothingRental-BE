using EXE201.Service.DTOs.OutfitImageDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class OutfitImageController : ControllerBase
{
    private readonly IOutfitImageService _outfitImageService;

    public OutfitImageController(IOutfitImageService outfitImageService)
    {
        _outfitImageService = outfitImageService;
    }

    /// <summary>
    /// Get all images for a specific outfit
    /// </summary>
    [HttpGet("outfit/{outfitId}")] // FIXED: Unique route
    [AllowAnonymous]
    public async Task<IActionResult> GetByOutfitId(int outfitId)
    {
        try
        {
            if (outfitId <= 0)
                return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

            var images = await _outfitImageService.GetImageByOutfitIdAsync(outfitId);
            return Ok(new { success = true, data = images });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Error retrieving images." });
        }
    }

    /// <summary>
    /// Get specific image by its unique ID
    /// </summary>
    [HttpGet("{id}")] // FIXED: Unique route
    [AllowAnonymous]
    public async Task<IActionResult> GetByImageId(int id)
    {
        try
        {
            var image = await _outfitImageService.GetByImageIdAsync(id);
            if (image == null) return NotFound(new { success = false, message = "Image not found" });
            return Ok(new { success = true, data = image });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Error retrieving image." });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddAsync([FromBody] CreateOutfitImageDto entity)
    {
        try
        {
            // Note: [ApiController] handles ModelState automatically
            var result = await _outfitImageService.AddAsync(entity);
            if (!result)
                return BadRequest(new { success = false, message = "Could not add image. Ensure the Outfit exists." });

            return StatusCode(201, new { success = true, message = "Image added successfully." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Error creating image." });
        }
    }

    [HttpPut("{id}")] // FIXED: Added {id} to the route for Update
    [Authorize]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateOutfitImageDto entity)
    {
        try
        {
            var result = await _outfitImageService.UpdateAsync(id, entity);
            if (!result)
                return NotFound(new { success = false, message = "Image not found or update failed." });

            return Ok(new { success = true, message = "Image updated successfully." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Error updating image." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            var result = await _outfitImageService.DeleteAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "Image not found." });

            // FIXED: Success message was "Image added successfully"
            return Ok(new { success = true, message = "Image deleted successfully." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Error deleting image." });
        }
    }
}