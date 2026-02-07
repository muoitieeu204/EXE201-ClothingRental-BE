using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.RentalPackageDTOs
{
    public class UpdateRentalPackageDto
    {
        public string Name { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public double PriceFactor { get; set; }
        public double? DepositPercent { get; set; }
        public decimal? OverdueFeePerHour { get; set; }
        public double? WeekendSurchargePercent { get; set; }
    }
}
