using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.ServicePackageDTOs;
using EXE201.Service.Interface;

namespace EXE201.Service.Implementation
{
    public class ServicePackageService : IServicePackageService
    {
    private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServicePackageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
  _unitOfWork = unitOfWork;
      _mapper = mapper;
        }

        public async Task<IEnumerable<ServicePackageResponseDto>> GetAllAsync()
        {
            var packages = await _unitOfWork.ServicePackages.GetAllAsync();
  return _mapper.Map<IEnumerable<ServicePackageResponseDto>>(packages);
        }

        public async Task<ServicePackageDetailDto?> GetByIdAsync(int id)
        {
  var package = await _unitOfWork.ServicePackages.GetByIdAsync(id);
          if (package == null) return null;
         return _mapper.Map<ServicePackageDetailDto>(package);
        }

        public async Task<IEnumerable<ServicePackageResponseDto>> GetPackagesByStudioIdAsync(int studioId)
        {
         var packages = await _unitOfWork.ServicePackages.GetPackagesByStudioIdAsync(studioId);
   return _mapper.Map<IEnumerable<ServicePackageResponseDto>>(packages);
      }

     public async Task<ServicePackageResponseDto?> CreateAsync(CreateServicePackageDto dto)
        {
            // Check if studio exists
            var studio = await _unitOfWork.Studios.GetByIdAsync(dto.StudioId);
        if (studio == null)
           return null;

            var package = _mapper.Map<ServicePackage>(dto);
      await _unitOfWork.ServicePackages.AddAsync(package);
            await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<ServicePackageResponseDto>(package);
        }

  public async Task<bool> UpdateAsync(int id, UpdateServicePackageDto dto)
        {
            var package = await _unitOfWork.ServicePackages.GetByIdAsync(id);
            if (package == null)
  return false;

            // If StudioId is being updated, verify the new studio exists
   if (dto.StudioId.HasValue && dto.StudioId.Value != package.StudioId)
   {
    var studio = await _unitOfWork.Studios.GetByIdAsync(dto.StudioId.Value);
      if (studio == null)
        return false;
      }

            _mapper.Map(dto, package);
 await _unitOfWork.ServicePackages.UpdateAsync(package);
      await _unitOfWork.SaveChangesAsync();

  return true;
        }

  public async Task<bool> DeleteAsync(int id)
        {
 var package = await _unitOfWork.ServicePackages.GetByIdAsync(id);
    if (package == null)
     return false;

          await _unitOfWork.ServicePackages.DeleteAsync(package);
            await _unitOfWork.SaveChangesAsync();

         return true;
    }

     public async Task<bool> ExistsAsync(int id)
        {
         var package = await _unitOfWork.ServicePackages.GetByIdAsync(id);
          return package != null;
  }
    }
}
