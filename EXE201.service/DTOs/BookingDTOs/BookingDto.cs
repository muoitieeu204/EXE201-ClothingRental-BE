using System;
using System.Collections.Generic;
using EXE201.Service.DTOs.ServiceBookingDTOs;

namespace EXE201.Service.DTOs.BookingDTOs
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }

        public int? AddressId { get; set; }
        public string? AddressText { get; set; }

        public decimal? TotalRentalAmount { get; set; }
        public decimal? TotalDepositAmount { get; set; }
        public decimal? TotalSurcharge { get; set; }
        public decimal? TotalServiceAmount { get; set; }
        public decimal? TotalOrderAmount { get; set; }

        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? BookingDate { get; set; }

        public List<BookingDetailDto> Details { get; set; } = new();
        public List<ServiceBookingResponseDto> ServiceBookings { get; set; } = new();
    }
}
