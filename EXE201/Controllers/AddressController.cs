using EXE201.Service.DTOs.AddressDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/Address")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out userId);
        }

        // GET /api/Address/get-all?includeInactive=false
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var result = await _addressService.GetMyAddressesAsync(userId, includeInactive);
            return Ok(result);
        }

        // GET /api/Address/get-by-id/{addressId}
        [HttpGet("get-by-id/{addressId:int}")]
        public async Task<IActionResult> GetById([FromRoute] int addressId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var result = await _addressService.GetMyAddressByIdAsync(userId, addressId);
            if (result == null)
                return NotFound(new { message = "Address not found" });

            return Ok(result);
        }

        // POST /api/Address/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            if (dto == null)
                return BadRequest(new { message = "Body is required" });

            var created = await _addressService.CreateMyAddressAsync(userId, dto);
            return Ok(created);
        }

        // PUT /api/Address/update/{addressId}
        [HttpPut("update/{addressId:int}")]
        public async Task<IActionResult> Update([FromRoute] int addressId, [FromBody] UpdateAddressDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            if (dto == null)
                return BadRequest(new { message = "Body is required" });

            var ok = await _addressService.UpdateMyAddressAsync(userId, addressId, dto);
            if (!ok)
                return NotFound(new { message = "Address not found or cannot be updated" });

            return Ok(new { message = "Address updated successfully" });
        }

        // DELETE /api/Address/delete/{addressId}  (soft delete)
        [HttpDelete("delete/{addressId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int addressId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var ok = await _addressService.DeleteMyAddressAsync(userId, addressId);
            if (!ok)
                return NotFound(new { message = "Address not found" });

            return Ok(new { message = "Address set to Inactive" });
        }

        // PATCH /api/Address/restore/{addressId}
        [HttpPatch("restore/{addressId:int}")]
        public async Task<IActionResult> Restore([FromRoute] int addressId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var ok = await _addressService.RestoreMyAddressAsync(userId, addressId);
            if (!ok)
                return NotFound(new { message = "Address not found" });

            return Ok(new { message = "Address set to Active" });
        }
    }
}
