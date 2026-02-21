using EXE201.Service.DTOs.LoyaltyTransactionDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoyaltyTransactionController : ControllerBase
    {
        private readonly ILoyaltyTransactionService _service;

        public LoyaltyTransactionController(ILoyaltyTransactionService service)
        {
            _service = service;
        }

        // GET: api/LoyaltyTransaction/all
        //[Authorize(Roles = "Admin", "Customer")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        // GET: api/LoyaltyTransaction/me
        // Lấy danh sách theo userId trong token
        [HttpGet("me")]
        public async Task<IActionResult> GetAllMe()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Token không hợp lệ (không tìm thấy user id)" });

            var data = await _service.GetAllMyAsync(userId);
            return Ok(data);
        }

        // GET: api/LoyaltyTransaction/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });

            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound(new { message = "Không tìm thấy giao dịch điểm" });

            return Ok(data);
        }

        // POST: api/LoyaltyTransaction/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateLoyaltyTransactionDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu body" });

            if (dto.UserId <= 0)
                return BadRequest(new { message = "UserId không hợp lệ" });

            if (string.IsNullOrWhiteSpace(dto.TransactionType))
                return BadRequest(new { message = "TransactionType không được để trống" });

            var created = await _service.CreateAsync(dto);
            if (created == null)
                return BadRequest(new { message = "Tạo giao dịch điểm thất bại (có thể user không tồn tại hoặc dữ liệu không hợp lệ)" });

            // Không có TransactionId trong DTO bạn chốt, nên CreatedAtAction theo id không hữu dụng lắm.
            // Return 201 là đủ.
            return StatusCode(201, created);
        }

        // PUT: api/LoyaltyTransaction/update/5
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateLoyaltyTransactionDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu body" });

            var hasAnyField =
                dto.PointsAmount.HasValue ||
                dto.TransactionType != null ||
                dto.Description != null;

            if (!hasAnyField)
                return BadRequest(new { message = "Không có dữ liệu để cập nhật" });

            if (dto.TransactionType != null && string.IsNullOrWhiteSpace(dto.TransactionType))
                return BadRequest(new { message = "TransactionType không được là chuỗi rỗng" });

            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return BadRequest(new { message = "Cập nhật thất bại (không tồn tại hoặc dữ liệu không hợp lệ)" });

            return Ok(updated);
        }

        // DELETE: api/LoyaltyTransaction/delete/5
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });

            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound(new { message = "Không tìm thấy giao dịch điểm" });

            return Ok(new { message = "Xóa giao dịch điểm thành công" });
        }
    }

}
