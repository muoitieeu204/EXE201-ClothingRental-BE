using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public decimal? TotalRentalAmount { get; set; }

    public decimal? TotalDepositAmount { get; set; }

    public decimal? TotalSurcharge { get; set; }

    public string? Status { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? BookingDate { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();

    public virtual ICollection<DepositTransaction> DepositTransactions { get; set; } = new List<DepositTransaction>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ServiceBooking> ServiceBookings { get; set; } = new List<ServiceBooking>();

    public virtual User User { get; set; } = null!;
}
