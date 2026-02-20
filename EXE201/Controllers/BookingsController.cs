using EXE201.Service.DTOs.BookingDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/Booking")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out userId);
        }

        // GET /api/Booking/get-all?includeDetails=true
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDetails = true)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var result = await _bookingService.GetMyBookingsAsync(userId, includeDetails);
            return Ok(result);
        }

        // GET /api/Booking/get-by-id/{bookingId}
        [HttpGet("get-by-id/{bookingId:int}")]
        public async Task<IActionResult> GetById([FromRoute] int bookingId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var result = await _bookingService.GetMyBookingByIdAsync(userId, bookingId);
            if (result == null) return NotFound(new { message = "Booking not found" });

            return Ok(result);
        }

        // POST /api/Booking/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            if (dto == null) return BadRequest(new { message = "Body is required" });

            var created = await _bookingService.CreateMyBookingAsync(userId, dto);
            if (created == null) return BadRequest(new { message = "Create booking failed (invalid data)" });

            return Ok(created);
        }

        [HttpPatch("complete/{bookingId:int}")]
        public async Task<IActionResult> Complete([FromRoute] int bookingId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var ok = await _bookingService.CompleteMyBookingAsync(userId, bookingId);
            if (!ok)
                return BadRequest(new { message = "Complete failed (not found, not yours, or not Pending)" });

            return Ok(new { message = "Booking completed successfully" });
        }


        // PATCH /api/Booking/cancel/{bookingId}
        [HttpPatch("cancel/{bookingId:int}")]
        public async Task<IActionResult> Cancel([FromRoute] int bookingId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { message = "Invalid token (missing user id)" });

            var ok = await _bookingService.CancelMyBookingAsync(userId, bookingId);
            if (!ok) return BadRequest(new { message = "Cancel failed (not found or not allowed)" });

            return Ok(new { message = "Booking cancelled successfully" });
        }
    }
}
