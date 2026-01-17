using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class DepositTransaction
{
    public int TransId { get; set; }

    public int BookingId { get; set; }

    public decimal? AmountHeld { get; set; }

    public decimal? AmountDeducted { get; set; }

    public string? DeductionReason { get; set; }

    public decimal? AmountRefunded { get; set; }

    public string? Status { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
