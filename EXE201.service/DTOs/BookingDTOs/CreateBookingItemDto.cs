using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.BookingDTOs
{
    //public class CreateBookingItemDto
    //{
    //    [Required]
    //    [Range(1, int.MaxValue)]
    //    public int OutfitSizeId { get; set; }

    //    // optional => không truyền thì null; gửi 0 thì backend tự normalize về null
    //    public int? RentalPackageId { get; set; }

    //    [Required]
    //    public DateTime StartTime { get; set; }

    //    // optional: nếu client muốn truyền thì lưu, không truyền thì null
    //    public DateTime? EndTime { get; set; }

    //    // client gửi giá => BE chỉ lưu + cộng tổng
    //    [Range(0, double.MaxValue)]
    //    public decimal UnitPrice { get; set; } = 0;

    //    [Range(0, double.MaxValue)]
    //    public decimal DepositAmount { get; set; } = 0;

    //    [Range(0, double.MaxValue)]
    //    public decimal Surcharge { get; set; } = 0;
    //}

}
