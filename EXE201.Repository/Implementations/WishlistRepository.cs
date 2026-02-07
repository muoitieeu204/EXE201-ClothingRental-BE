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
    public class WishlistRepository : GenericRepository<Wishlist>, IWishlistRepository
    {
        private readonly ClothingRentalDbContext _context;

        public WishlistRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Wishlist>> GetWishlistsByUserIdAsync(int userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }
    }
}
