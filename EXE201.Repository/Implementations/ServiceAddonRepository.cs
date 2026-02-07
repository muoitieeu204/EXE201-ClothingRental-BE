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
    public class ServiceAddonRepository : GenericRepository<ServiceAddon>, IServiceAddonRepository
    {
        private readonly ClothingRentalDbContext _context;

        public ServiceAddonRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceAddon>> GetAddonsByServicePackageIdAsync(int servicePkgId)
        {
            return await _context.ServiceAddons
                .Where(sa => sa.ServicePkgId == servicePkgId)
                .ToListAsync();
        }
    }
}
