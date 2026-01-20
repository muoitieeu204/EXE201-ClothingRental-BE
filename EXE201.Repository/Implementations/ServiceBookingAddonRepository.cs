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
    public class ServiceBookingAddonRepository : GenericRepository<ServiceBookingAddon>, IServiceBookingAddonRepository
    {
        private readonly ClothingRentalDbContext _context;

        public ServiceBookingAddonRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceBookingAddon>> GetAddonsByServiceBookingIdAsync(int svcBookingId)
        {
            return await _context.ServiceBookingAddons
                .Where(sba => sba.SvcBookingId == svcBookingId)
                .ToListAsync();
        }
    }
}
