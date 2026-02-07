using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.RentalPackageDTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class RentalPackageService : IRentalPackageService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public RentalPackageService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RentalPackageDto>> GetAllAsync()
        {
            var entities = await _uow.RentalPackages.GetAllAsync();
            return entities
                .OrderBy(x => x.DurationHours)
                .ThenBy(x => x.Name)
                .Select(x => _mapper.Map<RentalPackageDto>(x))
                .ToList();
        }

        public async Task<IEnumerable<RentalPackageSelectDto>> GetSelectListAsync()
        {
            var entities = await _uow.RentalPackages.GetAllAsync();
            return entities
                .OrderBy(x => x.DurationHours)
                .ThenBy(x => x.Name)
                .Select(x => _mapper.Map<RentalPackageSelectDto>(x))
                .ToList();
        }

        public async Task<RentalPackageDetailDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.RentalPackages.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<RentalPackageDetailDto>(entity);
        }

        public async Task<RentalPackageDetailDto?> CreateAsync(CreateRentalPackageDto dto)
        {
            // optional: enforce duration theo rule (24h / 72h)
            // if (dto.DurationHours != 24 && dto.DurationHours != 72) return null;

            // check trùng name
            var existed = await _uow.RentalPackages.ExistAsync(x => x.Name == dto.Name);
            if (existed) return null;

            var entity = _mapper.Map<RentalPackage>(dto);

            await _uow.RentalPackages.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<RentalPackageDetailDto>(entity);
        }

        public async Task<RentalPackageDetailDto?> UpdateAsync(int id, UpdateRentalPackageDto dto)
        {
            var entity = await _uow.RentalPackages.GetByIdAsync(id);
            if (entity == null) return null;

            // optional: enforce duration theo rule
            // if (dto.DurationHours != 24 && dto.DurationHours != 72) return null;

            // check name conflict
            var conflict = await _uow.RentalPackages.ExistAsync(x => x.Name == dto.Name && x.PackageId != id);
            if (conflict) return null;

            // map dto -> entity (nhờ AutoMapper)
            _mapper.Map(dto, entity);

            await _uow.RentalPackages.UpdateAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<RentalPackageDetailDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _uow.RentalPackages.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.RentalPackages.DeleteAsync(entity);

            try
            {
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
            {
                // dính FK (BookingDetails trỏ vào) thì sẽ fail
                return false;
            }
        }
    }
}
