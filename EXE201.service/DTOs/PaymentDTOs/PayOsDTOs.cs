namespace EXE201.Service.DTOs.PaymentDTOs
{
    public class CreatePayOsPaymentRequest
  {
        public int BookingId { get; set; }
        public string? PaymentType { get; set; } // "deposit" or "full"
    }

    public class PayOsPaymentResponse
    {
      public string? CheckoutUrl { get; set; }
        public long OrderCode { get; set; }
        public int BookingId { get; set; }
        public string? PaymentType { get; set; }
        public decimal Amount { get; set; }
    }

  public class PayOsWebhookData
    {
    public string? Code { get; set; }
        public string? Desc { get; set; }
        public bool Success { get; set; }
        public PayOsWebhookDataContent? Data { get; set; }
        public string? Signature { get; set; }
    }

  public class PayOsWebhookDataContent
    {
   public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
     public string? AccountNumber { get; set; }
        public string? Reference { get; set; }
    public string? TransactionDateTime { get; set; }
   public string? Currency { get; set; }
     public string? PaymentLinkId { get; set; }
        public string? Code { get; set; }
        public string? Desc { get; set; }
     public string? CounterAccountBankId { get; set; }
        public string? CounterAccountBankName { get; set; }
   public string? CounterAccountName { get; set; }
        public string? CounterAccountNumber { get; set; }
        public string? VirtualAccountName { get; set; }
    public string? VirtualAccountNumber { get; set; }
    }

    public class PayOsWebhookResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

 public class PayOsPaymentInfo
    {
      public long OrderCode { get; set; }
        public decimal Amount { get; set; }
   public decimal AmountPaid { get; set; }
        public decimal AmountRemaining { get; set; }
     public string? Status { get; set; }
        public string? CreatedAt { get; set; }
        public List<PayOsTransaction>? Transactions { get; set; }
        public string? CancellationReason { get; set; }
     public string? CanceledAt { get; set; }
    }

    public class PayOsLocalSyncResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public long OrderCode { get; set; }
        public int? BookingId { get; set; }
        public string? PayOsStatus { get; set; }
        public string? LocalPaymentStatus { get; set; }
        public string? BookingPaymentStatus { get; set; }
    }

    public class PayOsTransaction
    {
   public string? Reference { get; set; }
        public decimal Amount { get; set; }
   public string? AccountNumber { get; set; }
        public string? Description { get; set; }
     public string? TransactionDateTime { get; set; }
      public string? VirtualAccountName { get; set; }
     public string? VirtualAccountNumber { get; set; }
        public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
        public string? CounterAccountName { get; set; }
        public string? CounterAccountNumber { get; set; }
    }
}
