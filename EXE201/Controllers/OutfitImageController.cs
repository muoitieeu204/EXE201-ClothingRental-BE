using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using EXE201.Service.Implementation;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
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
        ///  Get all image for a specific outfit
        /// </summary>
        /// <param name="outfitId"></param>
        /// <returns>All image for that specific outfit</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByOutfitId([FromQuery] int outfitId)
        {
            try
            {
                if (outfitId <= 0)
                    return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

                var images = await _outfitImageService.GetImageByOutfitIdAsync(outfitId);
                return Ok(new { success = true, data = images });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving images." });
            }
        }

        /// <summary>
        /// Get specific image for a outfit
        /// </summary>
        /// <param name="id"></param>
        /// <returns>specific image for that outfit</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByImageId(int id)
        {
            try
            {
                var image = await _outfitImageService.GetByImageIdAsync(id);
                if (image == null) return NotFound(new { success = false, message = "Image not found" });
                return Ok(new {success = true, data = image});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving images." });
            }
        }

        /// <summary>
        /// Create image for specfic outfit
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Image for specific image create successfully</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddAsync([FromBody] OutfitImageDTO entity)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data provided." });

                var result = await _outfitImageService.AddAsync(entity);
                if (!result)
                    return BadRequest(new { success = false, message = "Could not add image. Ensure the Outfit exists." });

                return StatusCode(201, new { success = true, message = "Image added successfully." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error creating images." });
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody]OutfitImageDTO entity) 
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data provided." });

                var outfitImage = await _outfitImageService.GetByImageIdAsync(id);
                if(outfitImage == null)
                {
                    return NotFound(new {success = false, message = "Could not found Image"});
                }
                var result = await _outfitImageService.UpdateAsync(id, outfitImage);
                if (!result)
                    return BadRequest(new { success = false, message = "Could not add image. Ensure the Outfit exists." });

                return StatusCode(201, new { success = true, message = "Image added successfully." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving images." });
            }
        }
    }
}
