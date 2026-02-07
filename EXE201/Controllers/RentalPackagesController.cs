using EXE201.Service.DTOs.RentalPackageDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalPackagesController : ControllerBase
    {
        private readonly IRentalPackageService _service;

        public RentalPackagesController(IRentalPackageService service)
        {
            _service = service;
        }

        // GET: api/RentalPackages/getAll
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        // GET: api/RentalPackages/getSelect
        [HttpGet("getSelect")]
        public async Task<IActionResult> GetSelect()
        {
            var data = await _service.GetSelectListAsync();
            return Ok(data);
        }

        // GET: api/RentalPackages/getById/5
        [HttpGet("getById/{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null)
                return NotFound(new { message = "Rental package not found" });

            return Ok(data);
        }

        // POST: api/RentalPackages/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateRentalPackageDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Body is required" });

            var created = await _service.CreateAsync(dto);
            if (created == null)
                return Conflict(new { message = "Create failed (duplicate name or invalid data)" });

            return Ok(created);
        }

        // PUT: api/RentalPackages/update/5
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateRentalPackageDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Body is required" });

            var updated = await _service.UpdateAsync(id, dto);

            if (updated == null)
            {
                var existed = await _service.GetByIdAsync(id);
                if (existed == null)
                    return NotFound(new { message = "Rental package not found" });

                return Conflict(new { message = "Update failed (duplicate name or invalid data)" });
            }

            return Ok(updated);
        }

        // DELETE: api/RentalPackages/delete/5
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok)
                return BadRequest(new { message = "Delete failed (not found or referenced by other data)" });

            return Ok(new { message = "Deleted successfully" });
        }
    }
}
