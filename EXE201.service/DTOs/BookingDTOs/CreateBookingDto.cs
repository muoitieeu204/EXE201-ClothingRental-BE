using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE201.Service.DTOs.BookingDTOs
{
    public class CreateBookingDto
    {
        [Required]
        public int AddressId { get; set; }

        [Range(1, int.MaxValue)]
        public int RentalDays { get; set; } = 1;

        public List<CreateBookingItemDto> Items { get; set; } = new();

        public List<int> ServicePackageIds { get; set; } = new();
    }
}
