using EXE201.Service.DTOs.CategoryDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/Category/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _categoryService.GetAllAsync();
            return Ok(data);
        }

        // GET: api/Category/id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });

            var data = await _categoryService.GetByIdAsync(id);
            if (data == null) return NotFound(new { message = "Không tìm thấy danh mục" });

            return Ok(data);
        }

        // GET: api/Category/by-name?name=Kimono
        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Thiếu tham số name" });

            var data = await _categoryService.GetByNameAsync(name);
            if (data == null) return NotFound(new { message = "Không tìm thấy danh mục" });

            return Ok(data);
        }

        // POST: api/Category/create
        [Authorize(Policy = "SuperAdminOnly")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu body" });

            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest(new { message = "CategoryName không được để trống" });

            var created = await _categoryService.CreateAsync(dto);
            if (created == null)
                return BadRequest(new { message = "Tạo danh mục thất bại (có thể bị trùng tên hoặc dữ liệu không hợp lệ)" });

            return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, created);
        }

        // PUT: api/Category/update/5
        [Authorize(Policy = "SuperAdminOnly")]
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCategoryDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });
            if (dto == null) return BadRequest(new { message = "Thiếu dữ liệu body" });

            // Không cho update kiểu “gửi rỗng”
            var hasAnyField = dto.CategoryName != null || dto.Description != null;
            if (!hasAnyField)
                return BadRequest(new { message = "Không có dữ liệu để cập nhật" });

            // Nếu có truyền CategoryName thì validate basic
            if (dto.CategoryName != null && string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest(new { message = "CategoryName không được là chuỗi rỗng" });

            var updated = await _categoryService.UpdateAsync(id, dto);
            if (updated == null)
                return BadRequest(new { message = "Cập nhật thất bại (không tồn tại / trùng tên / dữ liệu không hợp lệ)" });

            return Ok(updated);
        }

        // DELETE: api/Category/delete/5
        [Authorize(Policy = "SuperAdminOnly")]
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id <= 0) return BadRequest(new { message = "Id không hợp lệ" });

            var ok = await _categoryService.DeleteAsync(id);
            if (!ok) return NotFound(new { message = "Không tìm thấy danh mục" });

            return Ok(new { message = "Xóa danh mục thành công" });
        }
    }
}
