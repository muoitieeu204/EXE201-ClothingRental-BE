using EXE201.Service.DTOs.AddressDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/user-addresses")]
    public class UserAddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public UserAddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out userId);
        }

        // GET /api/user-addresses
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var result = await _addressService.GetMyAddressesAsync(userId);

            return Ok(new
            {
                userIdFromToken = userId,
                count = result.Count(),
                data = result
            });
        }


        // GET /api/user-addresses/{addressId}
        [HttpGet("{addressId:int}")]
        public async Task<IActionResult> GetById([FromRoute] int addressId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var result = await _addressService.GetMyAddressByIdAsync(userId, addressId);
            if (result == null)
                return NotFound(new { message = "Address not found" });

            return Ok(result);
        }

        // POST /api/user-addresses
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            if (dto == null)
                return BadRequest(new { message = "Body is required" });

            var created = await _addressService.CreateMyAddressAsync(userId, dto);

            // REST style: trả 201 + Location
            return CreatedAtAction(nameof(GetById), new { addressId = created.AddressId }, created);
        }

        // PUT /api/user-addresses/{addressId}
        [HttpPut("{addressId:int}")]
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

        // PATCH /api/user-addresses/{addressId}/set-default
        [HttpPatch("{addressId:int}/set-default")]
        public async Task<IActionResult> SetDefault([FromRoute] int addressId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var result = await _addressService.SetMyDefaultAddressAsync(userId, addressId);
            if (result == null)
                return NotFound(new { message = "Address not found" });

            return Ok(result); // trả đúng cái vừa set default
        }

        // DELETE /api/user-addresses/{addressId}  (hard delete)
        [HttpDelete("{addressId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int addressId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var ok = await _addressService.DeleteMyAddressAsync(userId, addressId);
            if (!ok)
                return NotFound(new { message = "Address not found" });

            return Ok(new { message = "Address deleted successfully" });
        }
    }
}
