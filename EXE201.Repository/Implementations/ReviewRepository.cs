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
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly ClothingRentalDbContext _context;

        public ReviewRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Reviews
                .Include(r => r.ReviewImages)
                .ToListAsync();
        }

        public override async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.ReviewImages)
                .FirstOrDefaultAsync(r => r.ReviewId == id);
        }

        public async Task<IEnumerable<Review>> GetReviewsByOutfitIdAsync(int outfitId)
        {
            return await _context.Reviews
                .Include(r => r.ReviewImages)
                .Where(r => r.OutfitId == outfitId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            return await _context.Reviews
                .Include(r => r.ReviewImages)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
    }
}
