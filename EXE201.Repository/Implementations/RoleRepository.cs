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
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly ClothingRentalDbContext _context;

        public RoleRepository(ClothingRentalDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
        }
    }
}
