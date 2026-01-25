using EXE201.Service.DTOs.OutfitAttributeDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutfitAttributesController : ControllerBase
    {
        private readonly IOutfitAttributeService _service;

        public OutfitAttributesController(IOutfitAttributeService service)
        {
            _service = service;
        }

        // GET: /api/OutfitAttributes/getAll
        [HttpGet]
        [ActionName("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET: /api/OutfitAttributes/getById/5
        [HttpGet("{detailId:int}")]
        [ActionName("getById")]
        public async Task<IActionResult> GetById([FromRoute] int detailId)
        {
            var result = await _service.GetByIdAsync(detailId);
            if (result == null) return NotFound(new { message = "OutfitAttribute not found" });
            return Ok(result);
        }

        // GET: /api/OutfitAttributes/getByOutfitId/10
        [HttpGet("{outfitId:int}")]
        [ActionName("getByOutfitId")]
        public async Task<IActionResult> GetByOutfitId([FromRoute] int outfitId)
        {
            var result = await _service.GetByOutfitIdAsync(outfitId);
            if (result == null) return NotFound(new { message = "OutfitAttribute not found for this outfit" });
            return Ok(result);
        }

        // POST: /api/OutfitAttributes/create
        [HttpPost]
        [Authorize]
        [ActionName("create")]
        public async Task<IActionResult> Create([FromBody] CreateOutfitAttributeDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Body is required" });

            var created = await _service.CreateAsync(dto);
            if (created == null)
                return BadRequest(new { message = "This outfit already has attributes (duplicate OutfitId)" });

            return Ok(created);
        }

        // PUT: /api/OutfitAttributes/update/5
        [HttpPut("{detailId:int}")]
        [Authorize]
        [ActionName("update")]
        public async Task<IActionResult> Update([FromRoute] int detailId, [FromBody] UpdateOutfitAttributeDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Body is required" });

            var updated = await _service.UpdateAsync(detailId, dto);
            if (updated == null) return NotFound(new { message = "OutfitAttribute not found" });

            return Ok(updated);
        }

        // DELETE: /api/OutfitAttributes/delete/5
        [HttpDelete("{detailId:int}")]
        [Authorize]
        [ActionName("delete")]
        public async Task<IActionResult> Delete([FromRoute] int detailId)
        {
            var ok = await _service.DeleteAsync(detailId);
            if (!ok) return NotFound(new { message = "OutfitAttribute not found" });

            return Ok(new { message = "Deleted successfully" });
        }
    }
}
