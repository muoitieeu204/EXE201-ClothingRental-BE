using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.ServiceBookingDTOs
{
    public class CreateServiceBookingDto
    {
  [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "BookingId must be greater than 0")]
        public int? BookingId { get; set; }

        [Required(ErrorMessage = "ServicePkgId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ServicePkgId must be greater than 0")]
        public int ServicePkgId { get; set; }

        public DateTime? ServiceTime { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "TotalPrice must be non-negative")]
        public decimal? TotalPrice { get; set; }

     [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }
 }
}
