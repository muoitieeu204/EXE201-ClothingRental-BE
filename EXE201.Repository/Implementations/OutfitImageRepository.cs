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
    public class OutfitImageRepository : GenericRepository<OutfitImage>, IOutfitImageRepository
    {
        private readonly ClothingRentalDbContext _context;

        public OutfitImageRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OutfitImage>> GetImagesByOutfitIdAsync(int outfitId)
        {
            return await _context.OutfitImages
                .Where(oi => oi.OutfitId == outfitId)
                .ToListAsync();
        }
    }
}
