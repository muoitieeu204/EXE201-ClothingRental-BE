using EXE201.Service.DTOs.ServicePackageDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicePackageController : ControllerBase
    {
        private readonly IServicePackageService _servicePackageService;

        public ServicePackageController(IServicePackageService servicePackageService)
        {
            _servicePackageService = servicePackageService;
        }

        /// <summary>
        /// Get all service packages
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var packages = await _servicePackageService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    data = packages,
                    count = packages.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving service packages." });
            }
        }

        /// <summary>
        /// Get service package by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Invalid service package ID." });

            try
            {
                var package = await _servicePackageService.GetByIdAsync(id);
                if (package == null)
                    return NotFound(new { success = false, message = "Service package not found." });

                return Ok(new { success = true, data = package });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the service package." });
            }
        }

        /// <summary>
        /// Get service packages by studio ID
        /// </summary>
        [HttpGet("studio/{studioId}")]
        public async Task<IActionResult> GetByStudioId(int studioId)
        {
            if (studioId <= 0)
                return BadRequest(new { success = false, message = "Invalid studio ID." });

            try
            {
                var packages = await _servicePackageService.GetPackagesByStudioIdAsync(studioId);
                return Ok(new
                {
                    success = true,
                    data = packages,
                    count = packages.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving service packages." });
            }
        }

        /// <summary>
        /// Create a new service package (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateServicePackageDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data.", errors = ModelState });

            try
            {
                var package = await _servicePackageService.CreateAsync(request);
                if (package == null)
                    return BadRequest(new { success = false, message = "Failed to create service package. Studio may not exist." });

                return CreatedAtAction(nameof(GetById), new { id = package.ServicePkgId }, new
                {
                    success = true,
                    message = "Service package created successfully.",
                    data = package
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while creating the service package." });
            }
        }

        /// <summary>
        /// Update an existing service package (Admin/Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServicePackageDto request)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Invalid service package ID." });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data.", errors = ModelState });

            try
            {
                var result = await _servicePackageService.UpdateAsync(id, request);
                if (!result)
                    return NotFound(new { success = false, message = "Service package not found or studio doesn't exist." });

                return Ok(new { success = true, message = "Service package updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating the service package." });
            }
        }

        /// <summary>
        /// Delete a service package (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Invalid service package ID." });

            try
            {
                var result = await _servicePackageService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Service package not found." });

                return Ok(new { success = true, message = "Service package deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the service package." });
            }
        }

        /// <summary>
        /// Check if service package exists
        /// </summary>
        [HttpGet("{id}/exists")]
        public async Task<IActionResult> Exists(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "Invalid service package ID." });

            try
            {
                var exists = await _servicePackageService.ExistsAsync(id);
                return Ok(new { success = true, exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred." });
            }
        }
    }
}
