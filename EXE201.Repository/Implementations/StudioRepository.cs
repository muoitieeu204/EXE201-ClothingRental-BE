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
    public class StudioRepository : GenericRepository<Studio>, IStudioRepository
    {
        private readonly ClothingRentalDbContext _context;

        public StudioRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Studio>> GetActiveStudiosAsync()
        {
            return await _context.Studios
                .Where(s => s.IsActive == true)
                .ToListAsync();
        }
    }
}
