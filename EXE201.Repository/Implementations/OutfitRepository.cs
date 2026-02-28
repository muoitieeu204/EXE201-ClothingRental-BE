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

        public override async Task<IEnumerable<Outfit>> GetAllAsync()
        {
            return await _context.Outfits
                .Include(o => o.Category)
                .Include(o => o.OutfitSizes)
                .Include(o => o.OutfitImages)
                .Include(o => o.Reviews)
                .ToListAsync();
        }

        public override async Task<Outfit?> GetByIdAsync(int id)
        {
            return await _context.Outfits
                .Include(o => o.Category)
                .Include(o => o.OutfitSizes)
                .Include(o => o.OutfitImages)
                .Include(o => o.Reviews)
                .Include(o => o.OutfitAttribute)
                .FirstOrDefaultAsync(o => o.OutfitId == id);
        }

        public async Task<IEnumerable<Outfit>> GetOutfitsByCategoryIdAsync(int categoryId)
        {
            return await _context.Outfits
                .Include(o => o.Category)
                .Include(o => o.OutfitSizes)
                .Include(o => o.OutfitImages)
                .Include(o => o.Reviews)
                .Where(o => o.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Outfit>> GetAvailableOutfitsAsync()
        {
            return await _context.Outfits
                .Include(o => o.Category)
                .Include(o => o.OutfitSizes)
                .Include(o => o.OutfitImages)
                .Include(o => o.Reviews)
                .Where(o => o.Status == "Available")
                .ToListAsync();
        }
    }
}
