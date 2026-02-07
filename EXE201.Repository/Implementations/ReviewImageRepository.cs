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
    public class ReviewImageRepository : GenericRepository<ReviewImage>, IReviewImageRepository
    {
        private readonly ClothingRentalDbContext _context;

        public ReviewImageRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewImage>> GetImagesByReviewIdAsync(int reviewId)
        {
            return await _context.ReviewImages
                .Where(ri => ri.ReviewId == reviewId)
                .ToListAsync();
        }
    }
}
