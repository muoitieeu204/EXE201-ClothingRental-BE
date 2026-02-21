using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.StudioDTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class StudioService : IStudioService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public StudioService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // Chỉ lấy studio IsActive = 1
        public async Task<IEnumerable<StudioDto>> GetAllIsActiveAsync()
        {
            var studios = await _uow.Studios.FindAsync(s => s.IsActive == true);
            return _mapper.Map<IEnumerable<StudioDto>>(studios);
        }

        // Chỉ trả về nếu studio tồn tại và IsActive = 1
        public async Task<StudioDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;

            var studio = await _uow.Studios.GetByIdAsync(id);
            if (studio == null) return null;
            if (studio.IsActive != true) return null;

            return _mapper.Map<StudioDto>(studio);
        }

        // Create: default IsActive = 1
        public async Task<StudioDto?> CreateAsync(CreateStudioDto dto)
        {
            if (dto == null) return null;

            var name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return null;

            var entity = _mapper.Map<Studio>(dto);
            entity.Name = name;
            entity.IsActive = true; // default = 1

            await _uow.Studios.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<StudioDto>(entity);
        }

        public async Task<StudioDto?> UpdateAsync(int id, UpdateStudioDto dto)
        {
            if (id <= 0) return null;
            if (dto == null) return null;

            var studio = await _uow.Studios.GetByIdAsync(id);
            if (studio == null) return null;
            if (studio.IsActive != true) return null;

            // Không cho update kiểu gửi rỗng
            var hasAnyField = dto.Name != null || dto.Address != null || dto.ContactInfo != null;
            if (!hasAnyField) return null;

            if (dto.Name != null)
            {
                var newName = dto.Name.Trim();
                if (string.IsNullOrWhiteSpace(newName)) return null;
                studio.Name = newName;
            }

            if (dto.Address != null)
                studio.Address = dto.Address;

            if (dto.ContactInfo != null)
                studio.ContactInfo = dto.ContactInfo;

            await _uow.Studios.UpdateAsync(studio);
            await _uow.SaveChangesAsync();

            return _mapper.Map<StudioDto>(studio);
        }

        // Soft delete: set IsActive = 0
        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0) return false;

            var studio = await _uow.Studios.GetByIdAsync(id);
            if (studio == null) return false;
            if (studio.IsActive != true) return true; // đã inactive sẵn thì coi như ok

            studio.IsActive = false;

            await _uow.Studios.UpdateAsync(studio);
            await _uow.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            if (id <= 0) return false;

            var studio = await _uow.Studios.GetByIdAsync(id);
            if (studio == null) return false;

            if (studio.IsActive == true) return true; // đang active rồi thì coi như ok

            studio.IsActive = true;

            await _uow.Studios.UpdateAsync(studio);
            await _uow.SaveChangesAsync();

            return true;
        }
    }

}
