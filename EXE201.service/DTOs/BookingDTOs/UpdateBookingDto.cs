using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.BookingDTOs
{
    public class UpdateBookingDto
    {
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? BookingDate { get; set; }

        // để CRUD đủ (nhưng thường backend tự tính)
        public decimal? TotalRentalAmount { get; set; }
        public decimal? TotalDepositAmount { get; set; }
        public decimal? TotalSurcharge { get; set; }
    }

}
