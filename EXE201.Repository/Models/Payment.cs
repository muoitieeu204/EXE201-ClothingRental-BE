using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionRef { get; set; }

    public DateTime? PaymentTime { get; set; }

    public string? Status { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
