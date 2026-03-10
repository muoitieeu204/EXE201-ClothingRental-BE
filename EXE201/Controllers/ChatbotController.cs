using EXE201.Service.DTOs.ChatbotDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        /// <summary>
        /// Chat với AI tư vấn trang phục
        /// </summary>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatMessageRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { success = false, message = "Message is required." });

            try
            {
                var result = await _chatbotService.GetChatResponseAsync(request.Message.Trim(), request.History);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Chat failed: {ex.Message}" });
            }
        }
    }
}
