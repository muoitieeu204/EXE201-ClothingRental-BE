using EXE201.Service.DTOs.PaymentDTOs;

namespace EXE201.Service.Interface
{
    public interface IPayOsService
    {
    Task<PayOsPaymentResponse> CreatePaymentLink(CreatePayOsPaymentRequest request);
    Task<PayOsWebhookResponse> HandleWebhook(PayOsWebhookData webhookData);
        Task<bool> CancelPaymentLink(long orderCode);
        Task<PayOsPaymentInfo> GetPaymentInfo(long orderCode);
        Task<PayOsLocalSyncResponse> SyncPaymentStatusByOrderCode(long orderCode);
    }
}
