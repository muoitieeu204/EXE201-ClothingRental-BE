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
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        private readonly ClothingRentalDbContext _context;

        public BookingRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByAddressIdAsync(int addressId)
        {
            return await _context.Bookings
                .Where(b => b.AddressId == addressId)
                .ToListAsync();
        }
    }
}
