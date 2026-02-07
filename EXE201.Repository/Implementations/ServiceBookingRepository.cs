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

        public override async Task<ServiceBooking?> GetByIdAsync(int id)
        {
            return await _context.ServiceBookings
                .Include(sb => sb.User)
                .Include(sb => sb.Booking)
                .Include(sb => sb.ServicePkg)
                .ThenInclude(sp => sp.Studio)
                .Include(sb => sb.ServiceBookingAddons)
                .ThenInclude(sba => sba.Addon)
                .FirstOrDefaultAsync(sb => sb.SvcBookingId == id);
        }

        public override async Task<IEnumerable<ServiceBooking>> GetAllAsync()
        {
            return await _context.ServiceBookings
                .Include(sb => sb.User)
                .Include(sb => sb.Booking)
                .Include(sb => sb.ServicePkg)
                .ThenInclude(sp => sp.Studio)
                .Include(sb => sb.ServiceBookingAddons)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceBooking>> GetServiceBookingsByUserIdAsync(int userId)
        {
            return await _context.ServiceBookings
                .Include(sb => sb.User)
                .Include(sb => sb.Booking)
                .Include(sb => sb.ServicePkg)
                .ThenInclude(sp => sp.Studio)
                .Include(sb => sb.ServiceBookingAddons)
                .Where(sb => sb.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceBooking>> GetServiceBookingsByBookingIdAsync(int bookingId)
        {
            return await _context.ServiceBookings
                .Include(sb => sb.User)
                .Include(sb => sb.Booking)
                .Include(sb => sb.ServicePkg)
                .ThenInclude(sp => sp.Studio)
                .Include(sb => sb.ServiceBookingAddons)
                .Where(sb => sb.BookingId == bookingId)
                .ToListAsync();
        }
    }
}
