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
    public class LoyaltyTransactionRepository : GenericRepository<LoyaltyTransaction>, ILoyaltyTransactionRepository
    {
        private readonly ClothingRentalDbContext _context;

        public LoyaltyTransactionRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LoyaltyTransaction>> GetTransactionsByUserIdAsync(int userId)
        {
            return await _context.LoyaltyTransactions
                .Where(lt => lt.UserId == userId)
                .ToListAsync();
        }
    }
}
