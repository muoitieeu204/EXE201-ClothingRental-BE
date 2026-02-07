using EXE201.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Repository.Interfaces
{
    public interface IServicePackageRepository : IGenericRepository<ServicePackage>
    {
        Task<IEnumerable<ServicePackage>> GetPackagesByStudioIdAsync(int studioId);
    }
}
