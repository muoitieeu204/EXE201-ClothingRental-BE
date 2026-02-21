using EXE201.Service.DTOs.ServiceAddonDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IServiceAddonService
    {
        Task<IEnumerable<ServiceAddonDto>> GetAllAsync();
        Task<ServiceAddonDto?> GetByIdAsync(int addonId);
        Task<IEnumerable<ServiceAddonDto>> GetByServicePkgIdAsync(int servicePkgId);
        Task<ServiceAddonDto?> CreateAsync(CreateServiceAddonDto dto);
        Task<ServiceAddonDto?> UpdateAsync(int addonId, UpdateServiceAddonDto dto);
        Task<bool> DeleteAsync(int addonId);
    }

}
