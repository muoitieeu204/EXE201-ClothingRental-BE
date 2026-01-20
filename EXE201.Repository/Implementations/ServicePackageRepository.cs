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
    public class ServicePackageRepository : GenericRepository<ServicePackage>, IServicePackageRepository
    {
        private readonly ClothingRentalDbContext _context;

        public ServicePackageRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServicePackage>> GetPackagesByStudioIdAsync(int studioId)
        {
            return await _context.ServicePackages
                .Where(sp => sp.StudioId == studioId)
                .ToListAsync();
        }
    }
}
