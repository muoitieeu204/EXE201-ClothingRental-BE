using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.BookingDTOs
{
    public class BookingDetailAdminV2Dto
    {
        public int DetailId { get; set; }
        public int BookingId { get; set; }

        public int OutfitSizeId { get; set; }
        public int? RentalPackageId { get; set; }
        public string? RentalPackageName { get; set; }

        // outfit info (tuỳ entity OutfitSize/Outfit của cốt)
        public int? OutfitId { get; set; }
        public string? OutfitName { get; set; }
        public string? OutfitType { get; set; }
        public string? OutfitSizeLabel { get; set; }
        public string? OutfitImageUrl { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public int? RentalDays { get; set; }   // nếu BE đang tính thì fill, không thì null
        public decimal? UnitPrice { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal? LateFee { get; set; }
        public decimal? DamageFee { get; set; }

        public string Status { get; set; } = string.Empty;
    }

}
