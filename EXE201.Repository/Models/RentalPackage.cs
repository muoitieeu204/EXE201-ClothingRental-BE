using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class RentalPackage
{
    public int PackageId { get; set; }

    public string Name { get; set; } = null!;

    public int DurationHours { get; set; }

    public double PriceFactor { get; set; }

    public double? DepositPercent { get; set; }

    public decimal? OverdueFeePerHour { get; set; }

    public double? WeekendSurchargePercent { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
}
