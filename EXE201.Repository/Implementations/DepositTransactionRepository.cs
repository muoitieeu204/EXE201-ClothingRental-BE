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
    public class DepositTransactionRepository : GenericRepository<DepositTransaction>, IDepositTransactionRepository
    {
        private readonly ClothingRentalDbContext _context;

        public DepositTransactionRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DepositTransaction>> GetTransactionsByBookingIdAsync(int bookingId)
        {
            return await _context.DepositTransactions
                .Where(dt => dt.BookingId == bookingId)
                .ToListAsync();
        }
    }
}
