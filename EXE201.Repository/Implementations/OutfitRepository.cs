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
    public class OutfitRepository : GenericRepository<Outfit>, IOutfitRepository
    {
        private readonly ClothingRentalDbContext _context;

        public OutfitRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Outfit>> GetOutfitsByCategoryIdAsync(int categoryId)
        {
            return await _context.Outfits
                .Where(o => o.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Outfit>> GetAvailableOutfitsAsync()
        {
            return await _context.Outfits
                .Where(o => o.Status == "Available")
                .ToListAsync();
        }
    }
}
