using EXE201.Service.DTOs.PaymentDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayOsController : ControllerBase
    {
        private readonly IPayOsService _payOsService;

        public PayOsController(IPayOsService payOsService)
        {
            _payOsService = payOsService;
        }

        /// <summary>
        /// Create PayOS payment link from booking amount.
        /// paymentType: "deposit" | "full"
        /// </summary>
        [Authorize]
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePayOsPaymentRequest request)
        {
            try
            {
                if (request == null || request.BookingId <= 0)
                {
                    return BadRequest(new { message = "BookingId is required." });
                }

                var response = await _payOsService.CreatePaymentLink(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Handle PayOS webhook for payment status updates
        /// </summary>
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] PayOsWebhookData webhookData)
        {
            try
            {
                var response = await _payOsService.HandleWebhook(webhookData);

                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Webhook processing error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cancel a payment link
        /// </summary>
        [Authorize]
        [HttpPost("cancel/{orderCode}")]
        public async Task<IActionResult> CancelPaymentLink(long orderCode)
        {
            try
            {
                var success = await _payOsService.CancelPaymentLink(orderCode);

                if (success)
                {
                    return Ok(new { message = "Payment link cancelled successfully." });
                }

                return BadRequest(new { message = "Failed to cancel payment link." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error cancelling payment: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get payment information by order code
        /// </summary>
        [Authorize]
        [HttpGet("info/{orderCode}")]
        public async Task<IActionResult> GetPaymentInfo(long orderCode)
        {
            try
            {
                var paymentInfo = await _payOsService.GetPaymentInfo(orderCode);
                return Ok(paymentInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error getting payment info: {ex.Message}" });
            }
        }
    }
}
