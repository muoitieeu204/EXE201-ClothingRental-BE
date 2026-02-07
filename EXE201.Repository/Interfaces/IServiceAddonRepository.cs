using EXE201.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Repository.Interfaces
{
    public interface IServiceAddonRepository : IGenericRepository<ServiceAddon>
    {
        Task<IEnumerable<ServiceAddon>> GetAddonsByServicePackageIdAsync(int servicePkgId);
    }
}
