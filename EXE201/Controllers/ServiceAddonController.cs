using EXE201.Service.DTOs.ServiceAddonDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceAddonController : ControllerBase
    {
        private readonly IServiceAddonService _service;

        public ServiceAddonController(IServiceAddonService service)
        {
            _service = service;
        }

        // GET: api/ServiceAddon/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        // GET: api/ServiceAddon/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });

            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound(new { message = "Không tìm thấy dịch vụ addon" });

            return Ok(data);
        }

        // GET: api/ServiceAddon/by-servicePkg/3
        [HttpGet("by-servicePkg/{servicePkgId:int}")]
        public async Task<IActionResult> GetByServicePkgId([FromRoute] int servicePkgId)
        {
            if (servicePkgId <= 0) return BadRequest(new { message = "ServicePkgId không hợp lệ" });

            var data = await _service.GetByServicePkgIdAsync(servicePkgId);

            // Trả Ok([]) để UI dễ xử lý (không có addon thì list rỗng)
            return Ok(data);
        }

        // POST: api/ServiceAddon/create
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateServiceAddonDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu body" });

            if (dto.ServicePkgId <= 0)
                return BadRequest(new { message = "ServicePkgId không hợp lệ" });

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Tên addon (Name) không được để trống" });

            if (dto.Price < 0)
                return BadRequest(new { message = "Giá (Price) không được nhỏ hơn 0" });

            var created = await _service.CreateAsync(dto);
            if (created == null)
                return BadRequest(new { message = "Tạo addon thất bại (có thể ServicePackage không tồn tại / trùng tên / dữ liệu không hợp lệ)" });

            return StatusCode(201, created);
        }

        // PUT: api/ServiceAddon/update/5
        [HttpPut("update/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateServiceAddonDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu body" });

            var hasAnyField = dto.Name != null || dto.Price.HasValue || dto.ServicePkgId.HasValue;
            if (!hasAnyField)
                return BadRequest(new { message = "Không có dữ liệu để cập nhật" });

            if (dto.Name != null && string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Tên addon (Name) không được là chuỗi rỗng" });

            if (dto.Price.HasValue && dto.Price.Value < 0)
                return BadRequest(new { message = "Giá (Price) không được nhỏ hơn 0" });

            // Lưu ý: service hiện đang IGNORE ServicePkgId khi update (theo yêu cầu “khỏi chỉnh sửa id”).
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return BadRequest(new { message = "Cập nhật addon thất bại (không tồn tại / trùng tên / dữ liệu không hợp lệ)" });

            return Ok(updated);
        }

        // DELETE: api/ServiceAddon/delete/5
        [HttpDelete("delete/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });

            var ok = await _service.DeleteAsync(id);
            if (!ok)
                return BadRequest(new { message = "Xóa addon thất bại (không tồn tại hoặc đang được sử dụng)" });

            return Ok(new { message = "Xóa addon thành công" });
        }
    }
}
