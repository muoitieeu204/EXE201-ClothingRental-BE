using EXE201.Service.DTOs.LeonardoAIDTOs;
using EXE201.Service.Implementation;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeonardoAIController : ControllerBase
    {
        private readonly ILeonardoAIService leonardoAIService;
        public LeonardoAIController(ILeonardoAIService leonardoAIService)
        {
            this.leonardoAIService = leonardoAIService;
        }

        /// <summary>
        /// Generate video from text prompt with optional start/end frames
        /// </summary>
        [HttpPost("generate-video")]
        public async Task<IActionResult> GenerateVideo([FromBody] GenerateRequestVideoDTO request)
        {
            try
            {
                var result = await leonardoAIService.GenerateVideoAsync(request);
                return Ok(new
                {
                    success = true,
                    data = result,
                    message = "Video generation started. Use generationId to check status."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Check video generation status
        /// </summary>
        [HttpGet("status/{generationId}")]
        public async Task<IActionResult> GetVideoStatus(string generationId)
        {
            try
            {
                var status = await leonardoAIService.GetVideoStatusAsync(generationId);

                if (status.status == null)
                    return NotFound(new { success = false, message = "Generation not found" });

                return Ok(new { success = true, data = status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Generate video and wait for completion (blocking, max 3 minutes)
        /// </summary>
        [HttpPost("generate-video-sync")]
        public async Task<IActionResult> GenerateVideoSync([FromBody] GenerateRequestVideoDTO request)
        {
            try
            {
                var result = await leonardoAIService.GenerateVideoAsync(request);
                var videoUrl = await leonardoAIService.WaitForVideoCompletionAsync(result, 180);

                if (videoUrl == null)
                    return StatusCode(408, new
                    {
                        success = false,
                        message = "Video generation timeout or failed",
                        generationId = result
                    });

                return Ok(new
                {
                    success = true,
                    data = new { generationId = result, videoUrl },
                    message = "Video generated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
