using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class ServiceBookingAddon
{
    public int Id { get; set; }

    public int SvcBookingId { get; set; }

    public int AddonId { get; set; }

    public decimal? PriceAtBooking { get; set; }

    public virtual ServiceAddon Addon { get; set; } = null!;

    public virtual ServiceBooking SvcBooking { get; set; } = null!;
}
