using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Repository.Implementations
{
    public class ServiceBookingRepository : GenericRepository<ServiceBooking>, IServiceBookingRepository
    {
        private readonly ClothingRentalDbContext _context;

        public ServiceBookingRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceBooking>> GetServiceBookingsByUserIdAsync(int userId)
        {
            return await _context.ServiceBookings
                .Where(sb => sb.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceBooking>> GetServiceBookingsByBookingIdAsync(int bookingId)
        {
            return await _context.ServiceBookings
                .Where(sb => sb.BookingId == bookingId)
                .ToListAsync();
        }
    }
}
