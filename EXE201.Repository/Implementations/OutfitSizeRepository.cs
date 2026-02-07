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
    public class OutfitSizeRepository : GenericRepository<OutfitSize>, IOutfitSizeRepository
    {
        private readonly ClothingRentalDbContext _context;

        public OutfitSizeRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OutfitSize>> GetSizesByOutfitIdAsync(int outfitId)
        {
            return await _context.OutfitSizes
                .Where(os => os.OutfitId == outfitId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OutfitSize>> GetAvailableSizesByOutfitIdAsync(int outfitId)
        {
            return await _context.OutfitSizes
                .Where(os => os.OutfitId == outfitId && os.Status == "Available")
                .ToListAsync();
        }
    }
}
