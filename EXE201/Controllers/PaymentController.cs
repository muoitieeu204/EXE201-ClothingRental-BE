using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using EXE201.Service.DTOs.PaymentDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Security.Claims;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private const string DefaultFrontendBaseUrl = "http://localhost:3000";
        private readonly IVnpayService _vnPayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IVnpayService vnPayService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        /// <summary>
        /// Create VNPay URL from booking amount.
        /// paymentType: "deposit" | "full"
        /// </summary>
        [Authorize]
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentRequestDto request)
        {
            if (request == null || request.BookingId <= 0)
            {
                return BadRequest(new { message = "BookingId is required." });
            }

            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Invalid token (missing user id)." });
            }

            var paymentType = NormalizePaymentType(request.PaymentType);
            if (paymentType == null)
            {
                return BadRequest(new { message = "PaymentType must be 'deposit' or 'full'." });
            }

            var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                return NotFound(new { message = "Booking not found." });
            }

            if (booking.UserId != userId)
            {
                return Forbid();
            }

            if (IsBookingStatusInvalidForPayment(booking.Status))
            {
                return BadRequest(new { message = "Cannot create payment for this booking status." });
            }

            var summary = await GetBookingPaymentSummaryAsync(booking);
            var amountToPay = paymentType == "deposit"
                ? Math.Max(0m, summary.DepositAmount - Math.Min(summary.PaidAmount, summary.DepositAmount))
                : Math.Max(0m, summary.TotalOrderAmount - summary.PaidAmount);

            amountToPay = RoundCurrency(amountToPay);
            if (amountToPay <= 0)
            {
                return BadRequest(new
                {
                    message = paymentType == "deposit"
                        ? "Deposit is already paid for this booking."
                        : "This booking is already fully paid."
                });
            }

            var transactionRef = $"{request.BookingId}-{paymentType}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var paymentRecord = new Payment
            {
                BookingId = request.BookingId,
                Amount = amountToPay,
                PaymentMethod = "VNPay",
                TransactionRef = transactionRef,
                PaymentTime = null,
                Status = "Pending"
            };

            await _unitOfWork.Payments.AddAsync(paymentRecord);
            await _unitOfWork.SaveChangesAsync();

            var orderInfo = new OrderInfoDTO
            {
                OrderId = request.BookingId,
                Amount = Convert.ToInt64(amountToPay, CultureInfo.InvariantCulture),
                OrderDesc = paymentType == "deposit"
                    ? $"Thanh toán tiền cọc cho đơn hàng #{request.BookingId} - Sắc Việt"
                    : $"Thanh toán toàn bộ cho đơn hàng #{request.BookingId} - Sắc Việt",
                CreatedDate = DateTime.Now,
                Status = "Pending",
                PaymentTranId = transactionRef,
                PayStatus = "Pending"
            };

            var paymentUrl = _vnPayService.CreatePaymentURL(orderInfo, HttpContext);

            return Ok(new
            {
                Url = paymentUrl,
                BookingId = request.BookingId,
                PaymentType = paymentType,
                Amount = amountToPay,
                TransactionRef = transactionRef
            });
        }

        /// <summary>
        /// Browser redirect
        /// </summary>
        [AllowAnonymous]
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> PaymentCallback()
        {
            var responseData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            var isValidSignature = _vnPayService.ValidatePaymentResponse(responseData);
            TryExtractTransactionInfo(responseData, out var bookingId, out var paymentType);

            if (isValidSignature)
            {
                await TrySyncPaymentFromGatewayResponseAsync(responseData);
            }

            if (isValidSignature && IsPaymentSuccess(responseData))
            {
                return Redirect(BuildFrontendRedirectUrl("/payment-success", "success", bookingId, paymentType));
            }

            return Redirect(BuildFrontendRedirectUrl("/payment-failed", "fail", bookingId, paymentType));
        }

        /// <summary>
        /// VNPay IPN (server to server): update Payment + Booking.PaymentStatus
        /// </summary>
        [AllowAnonymous]
        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> HandleIpn()
        {
            var responseData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            if (!_vnPayService.ValidatePaymentResponse(responseData))
            {
                return Ok(new { RspCode = "97", Message = "Invalid Checksum" });
            }

            if (!responseData.TryGetValue("vnp_TxnRef", out var txnRef) || string.IsNullOrWhiteSpace(txnRef))
            {
                return Ok(new { RspCode = "01", Message = "Transaction not found" });
            }

            var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(p => p.TransactionRef == txnRef);
            if (payment == null)
            {
                return Ok(new { RspCode = "01", Message = "Transaction not found" });
            }

            if (!responseData.TryGetValue("vnp_Amount", out var vnpAmountRaw) ||
                !long.TryParse(vnpAmountRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var vnpAmountMinorUnit))
            {
                return Ok(new { RspCode = "04", Message = "Invalid amount" });
            }

            var callbackAmount = RoundCurrency(vnpAmountMinorUnit / 100m);
            if (callbackAmount != RoundCurrency(payment.Amount))
            {
                return Ok(new { RspCode = "04", Message = "Invalid amount" });
            }

            var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
            if (booking == null)
            {
                return Ok(new { RspCode = "01", Message = "Booking not found" });
            }

            if (IsPaymentSuccess(responseData))
            {
                payment.Status = "Paid";
                payment.PaymentMethod = "VNPay";
                payment.PaymentTime = ParseVnpayPayDate(responseData) ?? DateTime.Now;

                await _unitOfWork.Payments.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await UpdateBookingPaymentStatusAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }

            if (!IsPaidStatus(payment.Status))
            {
                payment.Status = "Failed";
                payment.PaymentMethod = "VNPay";
                payment.PaymentTime = DateTime.Now;

                await _unitOfWork.Payments.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await UpdateBookingPaymentStatusAsync(booking);
                await _unitOfWork.SaveChangesAsync();
            }

            return Ok(new { RspCode = "00", Message = "Payment failed recorded" });
        }

        private static string? NormalizePaymentType(string? paymentType)
        {
            if (string.IsNullOrWhiteSpace(paymentType))
            {
                return "deposit";
            }

            var normalized = paymentType.Trim().ToLowerInvariant();
            return normalized is "deposit" or "full" ? normalized : null;
        }

        private static bool IsPaidStatus(string? status)
        {
            return string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetUserIdFromTransactionRef(
            string? txnRef,
            out int bookingId,
            out string paymentType)
        {
            bookingId = 0;
            paymentType = string.Empty;

            if (string.IsNullOrWhiteSpace(txnRef))
            {
                return false;
            }

            var parts = txnRef.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                return false;
            }

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out bookingId))
            {
                bookingId = 0;
                return false;
            }

            var normalizedType = NormalizePaymentType(parts[1]);
            paymentType = normalizedType ?? string.Empty;
            return true;
        }

        private static void TryExtractTransactionInfo(
            IReadOnlyDictionary<string, string> responseData,
            out int bookingId,
            out string paymentType)
        {
            bookingId = 0;
            paymentType = string.Empty;

            if (!responseData.TryGetValue("vnp_TxnRef", out var txnRef))
            {
                return;
            }

            TryGetUserIdFromTransactionRef(txnRef, out bookingId, out paymentType);
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out userId);
        }

        private static bool IsPaymentSuccess(IReadOnlyDictionary<string, string> responseData)
        {
            if (!responseData.TryGetValue("vnp_ResponseCode", out var responseCode))
            {
                return false;
            }

            responseData.TryGetValue("vnp_TransactionStatus", out var transactionStatus);
            var successResponse = string.Equals(responseCode, "00", StringComparison.OrdinalIgnoreCase);
            var successTransaction = string.IsNullOrWhiteSpace(transactionStatus) ||
                                     string.Equals(transactionStatus, "00", StringComparison.OrdinalIgnoreCase);

            return successResponse && successTransaction;
        }

        private static DateTime? ParseVnpayPayDate(IReadOnlyDictionary<string, string> responseData)
        {
            if (!responseData.TryGetValue("vnp_PayDate", out var payDateRaw) || string.IsNullOrWhiteSpace(payDateRaw))
            {
                return null;
            }

            if (DateTime.TryParseExact(
                    payDateRaw.Trim(),
                    "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                return parsed;
            }

            return null;
        }

        private static decimal RoundCurrency(decimal amount)
        {
            return Math.Round(amount, 0, MidpointRounding.AwayFromZero);
        }

        private static bool IsBookingStatusInvalidForPayment(string? status)
        {
            var normalized = status?.Trim().ToLowerInvariant();
            return normalized is "cancelled" or "canceled" or "completed" or "complete";
        }

        private string BuildFrontendRedirectUrl(
            string path,
            string status,
            int bookingId,
            string? paymentType)
        {
            var frontendBaseUrl = _configuration["FrontendSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                frontendBaseUrl = DefaultFrontendBaseUrl;
            }

            var queryParts = new List<string>
            {
                $"status={Uri.EscapeDataString(status)}"
            };

            if (bookingId > 0)
            {
                queryParts.Add($"bookingId={bookingId.ToString(CultureInfo.InvariantCulture)}");
            }

            if (!string.IsNullOrWhiteSpace(paymentType))
            {
                queryParts.Add($"paymentType={Uri.EscapeDataString(paymentType)}");
            }

            return $"{frontendBaseUrl.TrimEnd('/')}{path}?{string.Join("&", queryParts)}";
        }

        /// <summary>
        /// Fallback sync when browser lands on return URL before/without IPN.
        /// Keep this idempotent so IPN can still process safely.
        /// </summary>
        private async Task TrySyncPaymentFromGatewayResponseAsync(IReadOnlyDictionary<string, string> responseData)
        {
            if (!responseData.TryGetValue("vnp_TxnRef", out var txnRef) || string.IsNullOrWhiteSpace(txnRef))
            {
                return;
            }

            var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(p => p.TransactionRef == txnRef);
            if (payment == null)
            {
                return;
            }

            if (!responseData.TryGetValue("vnp_Amount", out var vnpAmountRaw) ||
                !long.TryParse(vnpAmountRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var vnpAmountMinorUnit))
            {
                return;
            }

            var callbackAmount = RoundCurrency(vnpAmountMinorUnit / 100m);
            if (callbackAmount != RoundCurrency(payment.Amount))
            {
                return;
            }

            var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
            if (booking == null)
            {
                return;
            }

            if (IsPaymentSuccess(responseData))
            {
                if (!IsPaidStatus(payment.Status))
                {
                    payment.Status = "Paid";
                    payment.PaymentMethod = "VNPay";
                    payment.PaymentTime = ParseVnpayPayDate(responseData) ?? DateTime.Now;
                    await _unitOfWork.Payments.UpdateAsync(payment);
                    await _unitOfWork.SaveChangesAsync();
                }

                await UpdateBookingPaymentStatusAsync(booking);
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            if (!IsPaidStatus(payment.Status))
            {
                payment.Status = "Failed";
                payment.PaymentMethod = "VNPay";
                payment.PaymentTime = DateTime.Now;

                await _unitOfWork.Payments.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
            }

            await UpdateBookingPaymentStatusAsync(booking);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<(decimal TotalOrderAmount, decimal DepositAmount, decimal PaidAmount)> GetBookingPaymentSummaryAsync(Booking booking)
        {
            var totalRental = booking.TotalRentalAmount ?? 0m;
            var totalSurcharge = booking.TotalSurcharge ?? 0m;
            var serviceBookings = await _unitOfWork.ServiceBookings.GetServiceBookingsByBookingIdAsync(booking.BookingId);
            var totalService = serviceBookings.Sum(sb => sb.TotalPrice ?? 0m);

            var totalOrderAmount = RoundCurrency(totalRental + totalSurcharge + totalService);
            var depositAmount = booking.TotalDepositAmount.HasValue && booking.TotalDepositAmount.Value > 0
                ? RoundCurrency(booking.TotalDepositAmount.Value)
                : RoundCurrency(totalOrderAmount * 0.3m);

            var payments = await _unitOfWork.Payments.GetPaymentsByBookingIdAsync(booking.BookingId);
            var paidAmount = RoundCurrency(
                payments
                    .Where(p => IsPaidStatus(p.Status))
                    .Sum(p => p.Amount));

            return (totalOrderAmount, depositAmount, paidAmount);
        }

        private async Task UpdateBookingPaymentStatusAsync(Booking booking)
        {
            var summary = await GetBookingPaymentSummaryAsync(booking);

            if (summary.TotalOrderAmount > 0 && summary.PaidAmount >= summary.TotalOrderAmount)
            {
                booking.PaymentStatus = "Paid";
            }
            else if (summary.DepositAmount > 0 && summary.PaidAmount >= summary.DepositAmount)
            {
                booking.PaymentStatus = "DepositPaid";
            }
            else if (summary.PaidAmount > 0)
            {
                booking.PaymentStatus = "PartiallyPaid";
            }
            else
            {
                booking.PaymentStatus = "Unpaid";
            }

            await _unitOfWork.Bookings.UpdateAsync(booking);
        }
    }
}
