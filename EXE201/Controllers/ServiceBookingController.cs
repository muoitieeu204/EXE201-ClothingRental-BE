using EXE201.Service.DTOs.ServiceBookingDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceBookingController : ControllerBase
    {
        private readonly IServiceBookingService _serviceBookingService;

        public ServiceBookingController(IServiceBookingService serviceBookingService)
        {
            _serviceBookingService = serviceBookingService;
        }

        /// <summary>
        /// Get all service bookings (Admin/Manager only)
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var bookings = await _serviceBookingService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    data = bookings,
                    count = bookings.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving service bookings.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get service booking by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Invalid service booking ID." });

            try
            {
                var booking = await _serviceBookingService.GetByIdAsync(id);
                if (booking == null)
                    return NotFound(new { success = false, message = "Service booking not found." });

                // Check if user is authorized to view this booking
                var currentUserId = GetCurrentUserId();
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && userRole != "Manager" && booking.UserId != currentUserId)
                    return Forbid();

                return Ok(new { success = true, data = booking });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the service booking.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get service bookings by user ID (Current user or Admin/Manager)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            if (userId <= 0)
                return BadRequest(new { success = false, message = "Invalid user ID." });

            try
            {
                // Check if user is authorized
                var currentUserId = GetCurrentUserId();
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && userRole != "Manager" && userId != currentUserId)
                    return Forbid();

                var bookings = await _serviceBookingService.GetBookingsByUserIdAsync(userId);
                return Ok(new
                {
                    success = true,
                    data = bookings,
                    count = bookings.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving service bookings.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's service bookings
        /// </summary>
        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized(new { success = false, message = "Invalid user token." });

                var bookings = await _serviceBookingService.GetBookingsByUserIdAsync(userId);
                return Ok(new
                {
                    success = true,
                    data = bookings,
                    count = bookings.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving your bookings.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get service bookings by booking ID (Admin/Manager only)
        /// </summary>
        [HttpGet("booking/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> GetByBookingId(int bookingId)
        {
            if (bookingId <= 0)
                return BadRequest(new { success = false, message = "Invalid booking ID." });

            try
            {
                var bookings = await _serviceBookingService.GetBookingsByBookingIdAsync(bookingId);
                return Ok(new
                {
                    success = true,
                    data = bookings,
                    count = bookings.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving service bookings.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new service booking
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateServiceBookingDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data.", errors = ModelState });

            try
            {
                // Ensure user can only create booking for themselves unless admin/manager
                var currentUserId = GetCurrentUserId();
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && userRole != "Manager" && request.UserId != currentUserId)
                    return Forbid();

                var booking = await _serviceBookingService.CreateAsync(request);
                if (booking == null)
                    return BadRequest(new { success = false, message = "Failed to create service booking. User, service package, or booking may not exist." });

                return CreatedAtAction(nameof(GetById), new { id = booking.SvcBookingId }, new
                {
                    success = true,
                    message = "Service booking created successfully.",
                    data = booking
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while creating the service booking.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing service booking
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceBookingDto request)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Invalid service booking ID." });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data.", errors = ModelState });

            try
            {
                // Check if booking exists and user is authorized
                var existingBooking = await _serviceBookingService.GetByIdAsync(id);
                if (existingBooking == null)
                    return NotFound(new { success = false, message = "Service booking not found." });

                var currentUserId = GetCurrentUserId();
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && userRole != "Manager" && existingBooking.UserId != currentUserId)
                    return Forbid();

                var result = await _serviceBookingService.UpdateAsync(id, request);
                if (!result)
                    return NotFound(new { success = false, message = "Service booking not found or related entities don't exist." });

                return Ok(new { success = true, message = "Service booking updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating the service booking.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a service booking
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Invalid service booking ID." });

            try
            {
                // Check if booking exists and user is authorized
                var existingBooking = await _serviceBookingService.GetByIdAsync(id);
                if (existingBooking == null)
                    return NotFound(new { success = false, message = "Service booking not found." });

                var currentUserId = GetCurrentUserId();
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && userRole != "Manager" && existingBooking.UserId != currentUserId)
                    return Forbid();

                var result = await _serviceBookingService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Service booking not found." });

                return Ok(new { success = true, message = "Service booking deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the service booking.", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if service booking exists
        /// </summary>
        [HttpGet("{id}/exists")]
        public async Task<IActionResult> Exists(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Invalid service booking ID." });

            try
            {
                var exists = await _serviceBookingService.ExistsAsync(id);
                return Ok(new { success = true, exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred.", error = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
