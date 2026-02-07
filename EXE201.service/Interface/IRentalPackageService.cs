using EXE201.Service.DTOs.RentalPackageDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IRentalPackageService
    {
        Task<IEnumerable<RentalPackageDto>> GetAllAsync();
        Task<IEnumerable<RentalPackageSelectDto>> GetSelectListAsync();
        Task<RentalPackageDetailDto?> GetByIdAsync(int id);

        Task<RentalPackageDetailDto?> CreateAsync(CreateRentalPackageDto dto);
        Task<RentalPackageDetailDto?> UpdateAsync(int id, UpdateRentalPackageDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
