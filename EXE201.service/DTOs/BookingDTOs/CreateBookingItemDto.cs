using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.BookingDTOs
{
    public class CreateBookingItemDto
    {
        [Required]
        public int OutfitSizeId { get; set; }

        [Required]
        public int RentalPackageId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }
    }
}
