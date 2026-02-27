using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.BookingDTOs
{
    public class BookingAdminV2Dto
    {
        public int BookingId { get; set; }

        // user info (để hiện cột Khách hàng giống demo)
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // address snapshot
        public int? AddressId { get; set; }
        public string? AddressText { get; set; }

        // money
        public decimal? TotalRentalAmount { get; set; }
        public decimal? TotalDepositAmount { get; set; }
        public decimal? TotalSurcharge { get; set; }

        public decimal? TotalServiceAmount { get; set; }
        public decimal? TotalOrderAmount { get; set; }

        // status
        public string Status { get; set; } = string.Empty;          // Pending/Active/Completed/Cancelled
        public string PaymentStatus { get; set; } = string.Empty;   // Unpaid/Paid/Refunded...

        public DateTime? BookingDate { get; set; }

        public List<BookingDetailAdminV2Dto> Details { get; set; } = new();
        public List<ServiceBookingDto> Services { get; set; } = new();
    }
}
