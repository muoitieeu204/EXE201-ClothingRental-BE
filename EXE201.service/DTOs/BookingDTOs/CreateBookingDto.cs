using EXE201.Service.DTOs.ServiceBookingDTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.BookingDTOs
{
    public class CreateBookingDto
    {
        [Required]
        public int AddressId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateBookingItemDto> Items { get; set; } = new();
    }
}
