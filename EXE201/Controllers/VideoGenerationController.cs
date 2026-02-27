using EXE201.Service.DTOs.GeminiDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class VideoGenerationController : ControllerBase
    {
        private readonly IVideoGenerationService _videoGenerationService;

        public VideoGenerationController(IVideoGenerationService videoGenerationService)
        {
            _videoGenerationService = videoGenerationService;
        }

        /// <summary>
        /// Generate a video using outfit images as references
        /// </summary>
        [HttpPost("generate")]
        //[Authorize]
        public async Task<IActionResult> GenerateVideo([FromBody] VideoGenerationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var result = await _videoGenerationService.GenerateVideoAsync(request);

                if (result.Status == "failed")
                    return BadRequest(new { success = false, message = result.Message });

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = "Video generation started. Use the operationName to check status."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while generating video",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Check the status of a video generation operation
        /// </summary>
        [HttpGet("status/{operationName}")]
        //[Authorize]
        public async Task<IActionResult> GetVideoStatus(string operationName)
        {
            if (string.IsNullOrWhiteSpace(operationName))
                return BadRequest(new { success = false, message = "Operation name is required" });

            try
            {
                // Decode the operation name if it was URL encoded
                operationName = Uri.UnescapeDataString(operationName);

                var result = await _videoGenerationService.GetVideoStatusAsync(operationName);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while checking video status",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Download the generated video
        /// </summary>
        [HttpGet("download")]
        //[Authorize]
        public async Task<IActionResult> DownloadVideo([FromQuery] string videoUri)
        {
            if (string.IsNullOrWhiteSpace(videoUri))
                return BadRequest(new { success = false, message = "Video URI is required" });

            try
            {
                var videoBytes = await _videoGenerationService.DownloadVideoAsync(videoUri);

                return File(videoBytes, "video/mp4", $"generated_video_{DateTime.UtcNow:yyyyMMddHHmmss}.mp4");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while downloading video",
                    error = ex.Message
                });
            }
        }
    }
}
