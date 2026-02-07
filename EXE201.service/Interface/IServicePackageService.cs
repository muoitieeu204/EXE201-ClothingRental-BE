using EXE201.Service.DTOs.ServicePackageDTOs;

namespace EXE201.Service.Interface
{
    public interface IServicePackageService
    {
   Task<IEnumerable<ServicePackageResponseDto>> GetAllAsync();
        Task<ServicePackageDetailDto?> GetByIdAsync(int id);
        Task<IEnumerable<ServicePackageResponseDto>> GetPackagesByStudioIdAsync(int studioId);
  Task<ServicePackageResponseDto?> CreateAsync(CreateServicePackageDto dto);
        Task<bool> UpdateAsync(int id, UpdateServicePackageDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
