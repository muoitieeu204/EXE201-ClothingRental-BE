using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.PaymentDTOs
{
    public class CreatePaymentRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "BookingId must be greater than 0.")]
        public int BookingId { get; set; }

        // Allowed values: "deposit" | "full"
        public string PaymentType { get; set; } = "deposit";
    }
}
