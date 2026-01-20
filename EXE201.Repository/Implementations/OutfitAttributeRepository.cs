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
    public class OutfitAttributeRepository : GenericRepository<OutfitAttribute>, IOutfitAttributeRepository
    {
        private readonly ClothingRentalDbContext _context;

        public OutfitAttributeRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<OutfitAttribute?> GetAttributeByOutfitIdAsync(int outfitId)
        {
            return await _context.OutfitAttributes
                .FirstOrDefaultAsync(oa => oa.OutfitId == outfitId);
        }
    }
}
