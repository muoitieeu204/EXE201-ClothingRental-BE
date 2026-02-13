using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.BookingDTOs
{
    public class CreateBookingDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int AddressId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateBookingItemDto> Items { get; set; } = new();

        // optional, không có thì thôi
        public CreateBookingServiceDto? Service { get; set; }
    }

    public class CreateBookingItemDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int OutfitSizeId { get; set; }

        // ✅ optional => không truyền thì null; gửi 0 thì backend tự normalize về null
        public int? RentalPackageId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        // ✅ optional: nếu client muốn truyền thì lưu, không truyền thì null
        public DateTime? EndTime { get; set; }

        // ✅ client gửi giá => BE chỉ lưu + cộng tổng
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal DepositAmount { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal Surcharge { get; set; } = 0;
    }

    public class CreateBookingServiceDto
    {
        // ✅ optional => không truyền thì coi như không có service
        public int? ServicePkgId { get; set; }

        public DateTime? ServiceTime { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; } = 0;

        public List<CreateBookingServiceAddonDto> Addons { get; set; } = new();
    }

    public class CreateBookingServiceAddonDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int AddonId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PriceAtBooking { get; set; } = 0;
    }
}
