using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.ServiceAddonDTOs;
using EXE201.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class ServiceAddonService : IServiceAddonService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ServiceAddonService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceAddonDto>> GetAllAsync()
        {
            var entities = await _uow.ServiceAddons.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceAddonDto>>(entities);
        }

        public async Task<ServiceAddonDto?> GetByIdAsync(int addonId)
        {
            if (addonId <= 0) return null;

            var entity = await _uow.ServiceAddons.GetByIdAsync(addonId);
            if (entity == null) return null;

            return _mapper.Map<ServiceAddonDto>(entity);
        }
        public async Task<IEnumerable<ServiceAddonDto>> GetByServicePkgIdAsync(int servicePkgId)
        {
            if (servicePkgId <= 0) return Enumerable.Empty<ServiceAddonDto>();

            // optional: check ServicePackage tồn tại (để phân biệt "id sai" vs "không có addon")
            var pkg = await _uow.ServicePackages.GetByIdAsync(servicePkgId);
            if (pkg == null) return Enumerable.Empty<ServiceAddonDto>();

            var entities = await _uow.ServiceAddons.FindAsync(a => a.ServicePkgId == servicePkgId);
            return _mapper.Map<IEnumerable<ServiceAddonDto>>(entities);
        }
        public async Task<ServiceAddonDto?> CreateAsync(CreateServiceAddonDto dto)
        {
            if (dto == null) return null;

            var name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return null;

            if (dto.ServicePkgId <= 0) return null;
            if (dto.Price < 0) return null;

            // Check ServicePackage tồn tại
            var pkg = await _uow.ServicePackages.GetByIdAsync(dto.ServicePkgId);
            if (pkg == null) return null;

            // Optional: chặn trùng name trong cùng package
            var existed = await _uow.ServiceAddons.FindAsync(a =>
                a.ServicePkgId == dto.ServicePkgId &&
                a.Name != null &&
                a.Name.ToLower() == name.ToLower()
            );
            if (existed.Any()) return null;

            var entity = _mapper.Map<ServiceAddon>(dto);
            entity.AddonId = 0; // identity
            entity.Name = name;

            await _uow.ServiceAddons.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<ServiceAddonDto>(entity);
        }

        public async Task<ServiceAddonDto?> UpdateAsync(int addonId, UpdateServiceAddonDto dto)
        {
            if (addonId <= 0) return null;
            if (dto == null) return null;

            var entity = await _uow.ServiceAddons.GetByIdAsync(addonId);
            if (entity == null) return null;

            // Không cho update kiểu gửi rỗng
            var hasAnyField =
                dto.Name != null ||
                dto.Price.HasValue ||
                dto.ServicePkgId.HasValue;

            if (!hasAnyField) return null;

            if (dto.Name != null)
            {
                var newName = dto.Name.Trim();
                if (string.IsNullOrWhiteSpace(newName)) return null;

                // Optional: chặn trùng trong cùng package
                if (!string.Equals(entity.Name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    var existed = await _uow.ServiceAddons.FindAsync(a =>
                        a.ServicePkgId == entity.ServicePkgId &&
                        a.AddonId != entity.AddonId &&
                        a.Name != null &&
                        a.Name.ToLower() == newName.ToLower()
                    );
                    if (existed.Any()) return null;
                }

                entity.Name = newName;
            }

            if (dto.Price.HasValue)
            {
                if (dto.Price.Value < 0) return null;
                entity.Price = dto.Price.Value;
            }

            await _uow.ServiceAddons.UpdateAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<ServiceAddonDto>(entity);
        }

        public async Task<bool> DeleteAsync(int addonId)
        {
            if (addonId <= 0) return false;

            var entity = await _uow.ServiceAddons.GetByIdAsync(addonId);
            if (entity == null) return false;

            try
            {
                await _uow.ServiceAddons.DeleteAsync(entity);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                // thường là bị FK: ServiceBookingAddons đang tham chiếu addon này
                return false;
            }
        }
    }

}
