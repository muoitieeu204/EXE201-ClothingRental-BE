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
    public class RentalPackageRepository : GenericRepository<RentalPackage>, IRentalPackageRepository
    {
        private readonly ClothingRentalDbContext _context;

        public RentalPackageRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<RentalPackage?> GetPackageByNameAsync(string packageName)
        {
            return await _context.RentalPackages
                .FirstOrDefaultAsync(rp => rp.Name == packageName);
        }
    }
}
