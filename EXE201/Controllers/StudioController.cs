using EXE201.Service.DTOs.StudioDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudioController : ControllerBase
    {
        private readonly IStudioService _studioService;

        public StudioController(IStudioService studioService)
        {
            _studioService = studioService;
        }

        // GET: api/Studio/isActive
        // Chỉ lấy các studio có IsActive = 1
        [HttpGet("isActive")]
        public async Task<IActionResult> GetAllIsActive()
        {
            var data = await _studioService.GetAllIsActiveAsync();
            return Ok(data);
        }

        // GET: api/Studio/5
        // Chỉ trả về nếu studio tồn tại và IsActive = 1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });

            var data = await _studioService.GetByIdAsync(id);
            if (data == null) return NotFound(new { message = "Không tìm thấy studio (hoặc studio đã bị vô hiệu hóa)" });

            return Ok(data);
        }

        // POST: api/Studio/create
        // Create: service tự set IsActive = 1
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateStudioDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu body" });

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Tên studio (Name) không được để trống" });

            var created = await _studioService.CreateAsync(dto);
            if (created == null)
                return BadRequest(new { message = "Tạo studio thất bại (dữ liệu không hợp lệ)" });

            return CreatedAtAction(nameof(GetById), new { id = created.StudioId }, created);
        }

        // PUT: api/Studio/update/5
        // Update: chỉ update khi studio đang IsActive = 1
        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStudioDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu body" });

            // Không cho update kiểu gửi rỗng
            var hasAnyField = dto.Name != null || dto.Address != null || dto.ContactInfo != null;
            if (!hasAnyField)
                return BadRequest(new { message = "Không có dữ liệu để cập nhật" });

            // Nếu có truyền Name thì không cho chuỗi rỗng
            if (dto.Name != null && string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Tên studio (Name) không được là chuỗi rỗng" });

            var updated = await _studioService.UpdateAsync(id, dto);
            if (updated == null)
                return BadRequest(new { message = "Cập nhật thất bại (không tồn tại / đã bị vô hiệu hóa / dữ liệu không hợp lệ)" });

            return Ok(updated);
        }

        // DELETE: api/Studio/delete/5
        // Soft delete: IsActive = 0
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });

            var ok = await _studioService.DeleteAsync(id);
            if (!ok) return NotFound(new { message = "Không tìm thấy studio" });

            return Ok(new { message = "Vô hiệu hóa studio thành công (IsActive = 0)" });
        }

        // PUT: api/Studio/activate/5
        // Reactivate: IsActive = 1
        [Authorize(Roles = "Admin")]
        [HttpPut("activate/{id:int}")]
        public async Task<IActionResult> Activate([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });

            var ok = await _studioService.ActivateAsync(id);
            if (!ok) return NotFound(new { message = "Không tìm thấy studio" });

            return Ok(new { message = "Kích hoạt lại studio thành công (IsActive = 1)" });
        }
    }

}
