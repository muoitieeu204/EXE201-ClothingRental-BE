using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.BookingDTOs
{
    public class ServiceBookingAddonDto
    {
        public int AddonId { get; set; }
        public decimal? PriceAtBooking { get; set; }
    }

    public class ServiceBookingDto
    {
        public int SvcBookingId { get; set; }
        public int BookingId { get; set; }
        public int ServicePkgId { get; set; }
        public DateTime? ServiceTime { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? Status { get; set; }

        public List<ServiceBookingAddonDto> Addons { get; set; } = new();
    }

}
