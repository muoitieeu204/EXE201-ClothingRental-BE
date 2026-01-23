using EXE201.Service.DTOs;
using EXE201.Service.DTOs.UserDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id must be greater than 0" });

            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = $"User {id} not found" });

            return Ok(user);
        }

        // PUT: /api/users/5/profile
        [HttpPut("{id:int}/profile")]
        [Authorize]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id must be greater than 0" });

            var ok = await _userService.SoftDeleteAsync(id);
            if (!ok) return NotFound(new { message = $"User {id} not found" });

            return Ok(new { message = "Deactivated" });
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<UserDTO>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }


        // Đổi mật khẩu cho user đã đăng nhập
        [HttpPut("me/password")]
        [Authorize]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordNewOnlyDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Body is required" });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var ok = await _userService.ChangePasswordLoggedInAsync(userId, dto);
            if (!ok) return BadRequest(new { message = "Password validation failed" });

            return Ok(new { message = "Password changed successfully" });
        }

        // ==========================================
        // Gửi mã OTP thay đổi mật khẩu
        // POST: /api/users/password/otp
        // Body: { "email": "abc@gmail.com" }
        // ==========================================
        [HttpPost("password/otp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendChangePasswordOtp([FromBody] SendChangePasswordOtpDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required" });

            try
            {
                var ok = await _userService.SendChangePasswordOtpAsync(dto.Email);
                if (!ok) return BadRequest(new { message = "Cannot send OTP (invalid email / user not found / cooldown)" });

                return Ok(new { message = "OTP sent. Check your email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", detail = ex.Message });
            }
        }

        // ==========================================
        // (B2) Quên mật khẩu -> VERIFY OTP -> trả resetToken
        // POST: /api/users/password/otp/verify
        // Body: { "email": "...", "otp": "123456" }
        // ==========================================
        [HttpPost("password/otp/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Otp))
                return BadRequest(new { message = "Email & Otp are required" });

            try
            {
                var resetToken = await _userService.VerifyChangePasswordOtpAsync(dto.Email, dto.Otp);
                if (resetToken == null)
                    return BadRequest(new { message = "OTP invalid/expired" });

                return Ok(new { message = "OTP verified", resetToken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", detail = ex.Message });
            }
        }

        // ==========================================
        // (B3) Quên mật khẩu -> RESET PASSWORD (không cần mk cũ)
        // POST: /api/users/password/reset
        // Body: { "email": "...", "resetToken": "...", "newPassword": "...", "confirmPassword": "..." }
        // ==========================================
        [HttpPost("password/reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordAfterOtpDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Body is required" });

            try
            {
                var ok = await _userService.ResetPasswordAfterOtpAsync(dto);
                if (!ok) return BadRequest(new { message = "Reset token invalid/expired or password validation failed" });

                return Ok(new { message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", detail = ex.Message });
            }
        }
    }
}
