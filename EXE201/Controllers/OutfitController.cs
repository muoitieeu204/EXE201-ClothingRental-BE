using EXE201.Service.DTOs.OutfitDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutfitController : ControllerBase
    {
        private readonly IOutfitService _outfitService;

        public OutfitController(IOutfitService outfitService)
      {
          _outfitService = outfitService;
        }

        /// <summary>
   /// Get all outfits
        /// </summary>
    [HttpGet]
[AllowAnonymous]
        public async Task<IActionResult> GetAll()
  {
          try
   {
     var outfits = await _outfitService.GetAllAsync();
     return Ok(new { success = true, data = outfits, count = outfits.Count() });
         }
   catch (Exception)
     {
            return StatusCode(500, new { success = false, message = "Error retrieving outfits." });
    }
        }

     /// <summary>
        /// Get available outfits only
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
  public async Task<IActionResult> GetAvailable()
        {
try
            {
  var outfits = await _outfitService.GetAvailableOutfitsAsync();
    return Ok(new { success = true, data = outfits, count = outfits.Count() });
            }
 catch (Exception)
   {
 return StatusCode(500, new { success = false, message = "Error retrieving available outfits." });
            }
}

      /// <summary>
        /// Get outfits by category
        /// </summary>
      [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(int categoryId)
  {
            try
            {
       if (categoryId <= 0)
  return BadRequest(new { success = false, message = "Please provide a valid CategoryId." });

     var outfits = await _outfitService.GetOutfitsByCategoryIdAsync(categoryId);
                return Ok(new { success = true, data = outfits, count = outfits.Count() });
  }
    catch (Exception)
            {
  return StatusCode(500, new { success = false, message = "Error retrieving outfits by category." });
     }
        }

        /// <summary>
   /// Get outfit by ID (basic info)
        /// </summary>
 [HttpGet("{id}")]
        [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
        {
 try
      {
      if (id <= 0)
          return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

 var outfit = await _outfitService.GetByIdAsync(id);
  if (outfit == null)
    return NotFound(new { success = false, message = "Outfit not found." });

    return Ok(new { success = true, data = outfit });
          }
     catch (Exception)
  {
     return StatusCode(500, new { success = false, message = "Error retrieving outfit." });
   }
    }

      /// <summary>
     /// Get outfit details (includes images, sizes, reviews)
        /// </summary>
 [HttpGet("{id}/detail")]
        [AllowAnonymous]
   public async Task<IActionResult> GetDetailById(int id)
      {
    try
      {
       if (id <= 0)
        return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

        var outfit = await _outfitService.GetDetailByIdAsync(id);
        if (outfit == null)
            return NotFound(new { success = false, message = "Outfit not found." });

     return Ok(new { success = true, data = outfit });
            }
 catch (Exception)
   {
  return StatusCode(500, new { success = false, message = "Error retrieving outfit details." });
   }
     }

     /// <summary>
   /// Search outfits by name, type, gender, or region
 /// </summary>
    [HttpGet("search")]
 [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
   try
          {
      if (string.IsNullOrWhiteSpace(q))
           return BadRequest(new { success = false, message = "Please provide a search term." });

  var outfits = await _outfitService.SearchAsync(q);
    return Ok(new { success = true, data = outfits, count = outfits.Count() });
 }
            catch (Exception)
          {
       return StatusCode(500, new { success = false, message = "Error searching outfits." });
    }
  }

        /// <summary>
        /// Create a new outfit
 /// </summary>
   [HttpPost]
        [Authorize]
   public async Task<IActionResult> Create([FromBody] CreateOutfitDto dto)
        {
  try
     {
      var outfitId = await _outfitService.AddAsync(dto);
  if (outfitId == 0)
    return BadRequest(new { success = false, message = "Could not create outfit. Ensure the Category exists." });

        return StatusCode(201, new { success = true, message = "Outfit created successfully.", outfitId });
   }
  catch (Exception)
     {
return StatusCode(500, new { success = false, message = "Error creating outfit." });
      }
   }

        /// <summary>
   /// Update an existing outfit
  /// </summary>
 [HttpPut("{id}")]
 [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOutfitDto dto)
   {
 try
{
       if (id <= 0)
 return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

     var result = await _outfitService.UpdateAsync(id, dto);
    if (!result)
    return NotFound(new { success = false, message = "Outfit not found or update failed." });

         return Ok(new { success = true, message = "Outfit updated successfully." });
            }
    catch (Exception)
     {
         return StatusCode(500, new { success = false, message = "Error updating outfit." });
            }
        }

   /// <summary>
  /// Delete an outfit
        /// </summary>
[HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
   {
 try
   {
      if (id <= 0)
         return BadRequest(new { success = false, message = "Please provide a valid OutfitId." });

          var result = await _outfitService.DeleteAsync(id);
    if (!result)
           return NotFound(new { success = false, message = "Outfit not found." });

return Ok(new { success = true, message = "Outfit deleted successfully." });
 }
     catch (Exception)
   {
        return StatusCode(500, new { success = false, message = "Error deleting outfit." });
       }
 }
    }
}
