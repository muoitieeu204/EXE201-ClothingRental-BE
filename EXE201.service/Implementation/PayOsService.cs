using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.PaymentDTOs;
using EXE201.Service.Interface;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using System.Globalization;

namespace EXE201.Service.Implementation
{
    public class PayOsService : IPayOsService
    {
        private readonly PayOS _payOS;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public PayOsService(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;

            var clientId = _configuration["PayOs:ClientId"] ?? throw new ArgumentNullException("PayOs:ClientId");
            var apiKey = _configuration["PayOs:ApiKey"] ?? throw new ArgumentNullException("PayOs:ApiKey");
            var checksumKey = _configuration["PayOs:ChecksumKey"] ?? throw new ArgumentNullException("PayOs:ChecksumKey");

            _payOS = new PayOS(clientId, apiKey, checksumKey);
        }

        public async Task<PayOsPaymentResponse> CreatePaymentLink(CreatePayOsPaymentRequest request)
        {
            try
            {
                var paymentType = NormalizePaymentType(request.PaymentType);
                if (paymentType == null)
                {
                    throw new ArgumentException("PaymentType must be 'deposit' or 'full'.");
                }

                var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("Booking not found.");
                }

                if (IsBookingStatusInvalidForPayment(booking.Status))
                {
                    throw new InvalidOperationException("Cannot create payment for this booking status.");
                }

                var summary = await GetBookingPaymentSummaryAsync(booking);
                var amountToPay = paymentType == "deposit"
                       ? Math.Max(0m, summary.DepositAmount - Math.Min(summary.PaidAmount, summary.DepositAmount))
              : Math.Max(0m, summary.TotalOrderAmount - summary.PaidAmount);

                amountToPay = RoundCurrency(amountToPay);
                if (amountToPay <= 0)
                {
                    throw new InvalidOperationException(paymentType == "deposit"
                   ? "Deposit is already paid for this booking."
                           : "This booking is already fully paid.");
                }

                var orderCode = long.Parse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
                var transactionRef = $"{request.BookingId}-{paymentType}-{orderCode}";

                var description = paymentType == "deposit"
           ? $"Coc DH#{request.BookingId}"
                      : $"Thanh toan DH#{request.BookingId}";

                var returnUrl = BuildRedirectUrl(
                    _configuration["PayOs:ReturnUrl"] ?? "http://localhost:3000/payment-success",
                    request.BookingId,
                    paymentType,
                    "success");
                var cancelUrl = BuildRedirectUrl(
                    _configuration["PayOs:CancelUrl"] ?? "http://localhost:3000/payment-failed",
                    request.BookingId,
                    paymentType,
                    "fail");

                var items = new List<Net.payOS.Types.ItemData>
              {
           new Net.payOS.Types.ItemData(paymentType == "deposit" ? "Tien coc" : "Thanh toan toan bo", 1, (int)amountToPay)
 };

                var paymentData = new PaymentData(
              orderCode: orderCode,
                       amount: (int)amountToPay,
                      description: description,
               items: items,
                 cancelUrl: cancelUrl,
            returnUrl: returnUrl
               );

                var createPayment = await _payOS.createPaymentLink(paymentData);

                // Save payment record to database
                var paymentRecord = new Payment
                {
                    BookingId = request.BookingId,
                    Amount = amountToPay,
                    PaymentMethod = "PayOS",
                    TransactionRef = transactionRef,
                    PaymentTime = DateTime.UtcNow,
                    Status = "Pending"
                };

                await _unitOfWork.Payments.AddAsync(paymentRecord);
                await _unitOfWork.SaveChangesAsync();

                return new PayOsPaymentResponse
                {
                    CheckoutUrl = createPayment.checkoutUrl,
                    OrderCode = orderCode,
                    BookingId = request.BookingId,
                    PaymentType = paymentType,
                    Amount = amountToPay
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating PayOS payment link: {ex.Message}", ex);
            }
        }

        public async Task<PayOsWebhookResponse> HandleWebhook(PayOsWebhookData webhookData)
        {
            try
            {
                if (webhookData?.Data == null)
                {
                    return new PayOsWebhookResponse
                    {
                        Success = false,
                        Message = "Invalid webhook data"
                    };
                }

                var orderCode = webhookData.Data.OrderCode;
                var description = webhookData.Data.Description ?? "";

                // Extract booking info from description or reference
                if (!TryExtractBookingInfo(description, out var bookingId, out var paymentType))
                {
                    return new PayOsWebhookResponse
                    {
                        Success = false,
                        Message = "Cannot extract booking information"
                    };
                }

                var transactionRef = $"{bookingId}-{paymentType}-{orderCode}";
                var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(p => p.TransactionRef == transactionRef);

                if (payment == null)
                {
                    return new PayOsWebhookResponse
                    {
                        Success = false,
                        Message = "Payment not found"
                    };
                }

                var booking = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
                if (booking == null)
                {
                    return new PayOsWebhookResponse
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                if (IsBookingStatusInvalidForPayment(booking.Status))
                {
                    if (!IsPaidStatus(payment.Status))
                    {
                        payment.Status = "Failed";
                        payment.PaymentTime = DateTime.Now;
                        await _unitOfWork.Payments.UpdateAsync(payment);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    return new PayOsWebhookResponse
                    {
                        Success = true,
                        Message = "Booking is not eligible for payment"
                    };
                }

                if (webhookData.Code == "00" && webhookData.Success)
                {
                    payment.Status = "Paid";
                    payment.PaymentTime = DateTime.Now;
                    await _unitOfWork.Payments.UpdateAsync(payment);
                    await _unitOfWork.SaveChangesAsync();

                    await UpdateBookingPaymentStatusAsync(booking);
                    await _unitOfWork.SaveChangesAsync();

                    return new PayOsWebhookResponse
                    {
                        Success = true,
                        Message = "Payment processed successfully"
                    };
                }

                if (!IsPaidStatus(payment.Status))
                {
                    payment.Status = "Failed";
                    payment.PaymentTime = DateTime.Now;
                    await _unitOfWork.Payments.UpdateAsync(payment);
                    await _unitOfWork.SaveChangesAsync();

                    await UpdateBookingPaymentStatusAsync(booking);
                    await _unitOfWork.SaveChangesAsync();
                }

                return new PayOsWebhookResponse
                {
                    Success = true,
                    Message = "Payment failed recorded"
                };
            }
            catch (Exception ex)
            {
                return new PayOsWebhookResponse
                {
                    Success = false,
                    Message = $"Error processing webhook: {ex.Message}"
                };
            }
        }

        public async Task<bool> CancelPaymentLink(long orderCode)
        {
            try
            {
                var cancelData = await _payOS.cancelPaymentLink(orderCode);
                return cancelData != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PayOsPaymentInfo> GetPaymentInfo(long orderCode)
        {
            try
            {
                var paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

                return new PayOsPaymentInfo
                {
                    OrderCode = paymentInfo.orderCode,
                    Amount = paymentInfo.amount,
                    AmountPaid = paymentInfo.amountPaid,
                    AmountRemaining = paymentInfo.amountRemaining,
                    Status = paymentInfo.status,
                    CreatedAt = paymentInfo.createdAt,
                    Transactions = paymentInfo.transactions?.Select(t => new PayOsTransaction
                    {
                        Reference = t.reference,
                        Amount = t.amount,
                        AccountNumber = t.accountNumber,
                        Description = t.description,
                        TransactionDateTime = t.transactionDateTime,
                        VirtualAccountName = t.virtualAccountName,
                        VirtualAccountNumber = t.virtualAccountNumber,
                        CounterAccountBankId = t.counterAccountBankId,
                        CounterAccountBankName = t.counterAccountBankName,
                        CounterAccountName = t.counterAccountName,
                        CounterAccountNumber = t.counterAccountNumber
                    }).ToList(),
                    CancellationReason = paymentInfo.cancellationReason,
                    CanceledAt = paymentInfo.canceledAt
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting payment info: {ex.Message}", ex);
            }
        }

        public async Task<PayOsLocalSyncResponse> SyncPaymentStatusByOrderCode(long orderCode)
        {
            try
            {
                var paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);
                var localPayment = await _unitOfWork.Payments.FirstOrDefaultAsync(
                    p => p.TransactionRef != null && p.TransactionRef.EndsWith($"-{orderCode}"));

                if (localPayment == null)
                {
                    return new PayOsLocalSyncResponse
                    {
                        Success = false,
                        Message = "Local payment record not found for this orderCode.",
                        OrderCode = orderCode,
                        PayOsStatus = paymentInfo.status
                    };
                }

                var booking = await _unitOfWork.Bookings.GetByIdAsync(localPayment.BookingId);
                if (booking == null)
                {
                    return new PayOsLocalSyncResponse
                    {
                        Success = false,
                        Message = "Booking not found for local payment record.",
                        OrderCode = orderCode,
                        BookingId = localPayment.BookingId,
                        PayOsStatus = paymentInfo.status,
                        LocalPaymentStatus = localPayment.Status
                    };
                }

                var normalizedPayOsStatus = NormalizePayOsStatus(paymentInfo.status);
                var payOsMarkedPaid =
                    normalizedPayOsStatus == "paid" ||
                    (paymentInfo.amount > 0 && paymentInfo.amountPaid >= paymentInfo.amount);
                var payOsMarkedFailed =
                    normalizedPayOsStatus is "cancelled" or "canceled" or "expired";

                if (payOsMarkedPaid)
                {
                    if (!IsPaidStatus(localPayment.Status))
                    {
                        localPayment.Status = "Paid";
                        localPayment.PaymentTime = DateTime.Now;
                        await _unitOfWork.Payments.UpdateAsync(localPayment);
                    }
                }
                else if (payOsMarkedFailed)
                {
                    if (!IsPaidStatus(localPayment.Status) &&
                        !string.Equals(localPayment.Status, "Failed", StringComparison.OrdinalIgnoreCase))
                    {
                        localPayment.Status = "Failed";
                        localPayment.PaymentTime = DateTime.Now;
                        await _unitOfWork.Payments.UpdateAsync(localPayment);
                    }
                }
                else if (!IsPaidStatus(localPayment.Status) &&
                         !string.Equals(localPayment.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                {
                    localPayment.Status = "Pending";
                    await _unitOfWork.Payments.UpdateAsync(localPayment);
                }

                await _unitOfWork.SaveChangesAsync();

                await UpdateBookingPaymentStatusAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                return new PayOsLocalSyncResponse
                {
                    Success = true,
                    Message = "Local payment status synced from PayOS.",
                    OrderCode = orderCode,
                    BookingId = booking.BookingId,
                    PayOsStatus = paymentInfo.status,
                    LocalPaymentStatus = localPayment.Status,
                    BookingPaymentStatus = booking.PaymentStatus
                };
            }
            catch (Exception ex)
            {
                return new PayOsLocalSyncResponse
                {
                    Success = false,
                    Message = $"Error syncing payment status: {ex.Message}",
                    OrderCode = orderCode
                };
            }
        }

        // Helper methods
        private static string BuildRedirectUrl(string baseUrl, int bookingId, string paymentType, string status)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return baseUrl;
            }

            var queryParams = new List<string>();

            void AddQueryParam(string key, string? value)
            {
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                queryParams.Add(
                    $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
            }

            AddQueryParam("bookingId", bookingId.ToString(CultureInfo.InvariantCulture));
            AddQueryParam("paymentType", paymentType);
            AddQueryParam("status", status);

            if (queryParams.Count == 0)
            {
                return baseUrl;
            }

            var separator = baseUrl.Contains('?') ? "&" : "?";
            return $"{baseUrl}{separator}{string.Join("&", queryParams)}";
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

        private static string NormalizePayOsStatus(string? status)
        {
            return (status ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool IsBookingStatusInvalidForPayment(string? status)
        {
            var normalized = status?.Trim().ToLowerInvariant();
            return normalized is "cancelled" or "canceled" or "completed" or "complete";
        }

        private static decimal RoundCurrency(decimal amount)
        {
            return Math.Round(amount, 0, MidpointRounding.AwayFromZero);
        }

        private static bool TryExtractBookingInfo(string description, out int bookingId, out string paymentType)
        {
            bookingId = 0;
            paymentType = string.Empty;

            try
            {
                paymentType = description.Contains("coc", StringComparison.OrdinalIgnoreCase) ? "deposit" : "full";

                // PayOS may sanitize special chars in description, so support both:
                // "Coc DH#20" and "Coc DH20"
                var source = description ?? string.Empty;
                var dhIndex = source.IndexOf("DH", StringComparison.OrdinalIgnoreCase);
                if (dhIndex >= 0)
                {
                    var start = dhIndex + 2;

                    while (start < source.Length &&
                           (source[start] == '#' || source[start] == ' ' || source[start] == ':' || source[start] == '-'))
                    {
                        start++;
                    }

                    var bookingIdStr = new string(
                        source
                            .Skip(start)
                            .TakeWhile(char.IsDigit)
                            .ToArray());

                    if (int.TryParse(bookingIdStr, out bookingId))
                    {
                        return true;
                    }
                }

                // Fallback to legacy format with '#'
                var parts = source.Split('#');
                if (parts.Length >= 2)
                {
                    var bookingIdStr = new string(parts[1].TakeWhile(char.IsDigit).ToArray());
                    if (int.TryParse(bookingIdStr, out bookingId))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
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

public class ItemData
{
    public string name { get; set; }
    public int quantity { get; set; }
    public int price { get; set; }

    public ItemData(string name, int quantity, int price)
    {
        this.name = name;
        this.quantity = quantity;
        this.price = price;
    }
}
