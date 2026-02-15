using EXE201.Service.DTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVnpayService _vnPayService;
        // Khuyến nghị: Sử dụng Repository hoặc UnitOfWork để truy xuất dữ liệu thật
        // private readonly IUnitOfWork _unitOfWork; 

        public PaymentController(IVnpayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        /// <summary>
        /// 1. Endpoint tạo URL thanh toán để gửi cho Frontend
        /// </summary>
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] int bookingId)
        {
      
            var orderInfo = new OrderInfoDTO
            {
                OrderId = bookingId,
                Amount = 7500000, 
                OrderDesc = $"Thanh toán tiền cọc cho đơn hàng #{bookingId} - Sắc Việt",
                CreatedDate = DateTime.Now,
                Status = "Pending"
            };

            var paymentUrl = _vnPayService.CreatePaymentURL(orderInfo, HttpContext);
            return Ok(new { Url = paymentUrl });
        }

        /// <summary>
        /// Browser redirect
        /// </summary>
        [HttpGet("vnpay-return")]
        public IActionResult PaymentCallback()
        {
            var responseData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            // Kiểm tra chữ ký bảo mật
            bool isValidSignature = _vnPayService.ValidatePaymentResponse(responseData);

            if (isValidSignature)
            {
                string vnp_ResponseCode = responseData["vnp_ResponseCode"];

                if (vnp_ResponseCode == "00")
                {
                
                    return Redirect("http://localhost:3000/payment-success?status=success");
                }
            }

            return Redirect("http://localhost:3000/payment-failed?status=fail");
        }

        /// <summary>
        /// 3. Endpoint IPN (Server-to-Server)
        /// </summary>
        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> HandleIpn()
        {
            var responseData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            if (_vnPayService.ValidatePaymentResponse(responseData))
            {
                // TODO: Thực hiện logic cập nhật Database tại đây
                // 1. Kiểm tra trạng thái đơn hàng trong DB
                // 2. Cập nhật bảng Payment sang trạng thái "Paid"

                // Trả về JSON theo đúng định dạng VNPAY yêu cầu
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }

            return Ok(new { RspCode = "97", Message = "Invalid Checksum" });
        }
    }
}
