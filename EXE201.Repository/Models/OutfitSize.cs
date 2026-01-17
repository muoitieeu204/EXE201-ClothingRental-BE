using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class OutfitSize
{
    public int SizeId { get; set; }

    public int OutfitId { get; set; }

    public string SizeLabel { get; set; } = null!;

    public int? StockQuantity { get; set; }

    public double? ChestMaxCm { get; set; }

    public double? WaistMaxCm { get; set; }

    public double? HipMaxCm { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();

    public virtual Outfit Outfit { get; set; } = null!;
}
