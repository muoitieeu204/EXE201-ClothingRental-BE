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
    public class BookingDetailRepository : GenericRepository<BookingDetail>, IBookingDetailRepository
    {
        private readonly ClothingRentalDbContext _context;

        public BookingDetailRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookingDetail>> GetDetailsByBookingIdAsync(int bookingId)
        {
            return await _context.BookingDetails
                .Where(bd => bd.BookingId == bookingId)
                .ToListAsync();
        }
    }
}
