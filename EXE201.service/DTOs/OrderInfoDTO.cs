using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs
{
    public class OrderInfoDTO
    {
        public int OrderId { get; set; } // Maps to Booking.BookingId
        public long Amount { get; set; } // Maps to Payment.Amount (in VND, multiplied by 100)
        public string OrderDesc { get; set; } // Description of the booking
        public DateTime CreatedDate { get; set; } // Maps to Booking.BookingDate
        public string Status { get; set; } // Maps to Booking.Status or Payment.Status
        public string PaymentTranId { get; set; } // Maps to Payment.TransactionRef
        public string BankCode { get; set; } // Optional: Bank code used for payment
        public string PayStatus { get; set; } // Maps to Payment.Status
    }
}
