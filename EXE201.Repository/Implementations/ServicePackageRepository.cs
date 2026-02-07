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

        public override async Task<ServicePackage?> GetByIdAsync(int id)
        {
            return await _context.ServicePackages
                .Include(sp => sp.Studio)
                .Include(sp => sp.ServiceAddons)
                .Include(sp => sp.ServiceBookings)
                .FirstOrDefaultAsync(sp => sp.ServicePkgId == id);
        }

        public override async Task<IEnumerable<ServicePackage>> GetAllAsync()
        {
            return await _context.ServicePackages
                .Include(sp => sp.Studio)
                .Include(sp => sp.ServiceAddons)
                .Include(sp => sp.ServiceBookings)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServicePackage>> GetPackagesByStudioIdAsync(int studioId)
        {
            return await _context.ServicePackages
                .Include(sp => sp.Studio)
                .Include(sp => sp.ServiceAddons)
                .Include(sp => sp.ServiceBookings)
                .Where(sp => sp.StudioId == studioId)
                .ToListAsync();
        }
    }
}
