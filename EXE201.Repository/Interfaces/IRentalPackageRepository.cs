using EXE201.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Repository.Interfaces
{
    public interface IRentalPackageRepository : IGenericRepository<RentalPackage>
    {
        Task<RentalPackage?> GetPackageByNameAsync(string packageName);
    }
}
