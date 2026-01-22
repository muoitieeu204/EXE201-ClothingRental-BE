using EXE201.Service.DTOs;
using EXE201.Service.DTOs.UserDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id must be greater than 0" });

            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = $"User {id} not found" });

            return Ok(user);
        }

        // PUT: /api/users/5/profile
        [HttpPut("{id:int}/profile")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserProfileDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Id must be greater than 0" });
            if (dto == null) return BadRequest(new { message = "Body is required" });

            var updated = await _userService.UpdateProfileAsync(id, dto);
            if (updated == null) return NotFound(new { message = $"User {id} not found (or inactive)" });

            return Ok(updated);
        }

        // DELETE: /api/users/5  (soft delete)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id must be greater than 0" });

            var ok = await _userService.SoftDeleteAsync(id);
            if (!ok) return NotFound(new { message = $"User {id} not found" });

            return NoContent(); // hoặc Ok(new { message = "Deactivated" })
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<UserDTO>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }
    }
}
