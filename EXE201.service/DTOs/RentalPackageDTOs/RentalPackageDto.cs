using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.RentalPackageDTOs
{
    // Dùng cho list (grid/table)
    public class RentalPackageDto
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = string.Empty;

        public int DurationHours { get; set; }
        public double PriceFactor { get; set; }

        public double? DepositPercent { get; set; }
        public decimal? OverdueFeePerHour { get; set; }
        public double? WeekendSurchargePercent { get; set; }
    }
}
