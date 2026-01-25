using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.OutfitAttributeDTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class OutfitAttributeService : IOutfitAttributeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OutfitAttributeService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OutfitAttributeDto>> GetAllAsync()
        {
            var list = await _uow.OutfitAttributes.GetAllAsync();
            return _mapper.Map<IEnumerable<OutfitAttributeDto>>(list);
        }

        public async Task<OutfitAttributeDto?> GetByIdAsync(int detailId)
        {
            var entity = await _uow.OutfitAttributes.GetByIdAsync(detailId);
            return entity == null ? null : _mapper.Map<OutfitAttributeDto>(entity);
        }

        public async Task<OutfitAttributeDto?> GetByOutfitIdAsync(int outfitId)
        {
            var entity = await _uow.OutfitAttributes.GetAttributeByOutfitIdAsync(outfitId);
            return entity == null ? null : _mapper.Map<OutfitAttributeDto>(entity);
        }

        public async Task<OutfitAttributeDto?> CreateAsync(CreateOutfitAttributeDto dto)
        {
            // Vì repo có GetAttributeByOutfitIdAsync (singular) -> ngầm hiểu 1 outfit chỉ có 1 attribute
            var existed = await _uow.OutfitAttributes.ExistAsync(x => x.OutfitId == dto.OutfitId);
            if (existed) return null;

            var entity = _mapper.Map<OutfitAttribute>(dto);

            await _uow.OutfitAttributes.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<OutfitAttributeDto>(entity);
        }

        public async Task<OutfitAttributeDto?> UpdateAsync(int detailId, UpdateOutfitAttributeDto dto)
        {
            var entity = await _uow.OutfitAttributes.GetByIdAsync(detailId);
            if (entity == null) return null;

            // Nếu DTO của bạn dùng nullable và bạn đã config AutoMapper Condition(null) thì dùng dòng này:
            _mapper.Map(dto, entity);

            // Nếu bạn CHƯA set AutoMapper condition, thì comment dòng trên và dùng update tay kiểu if(dto.X != null)...

            await _uow.OutfitAttributes.UpdateAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<OutfitAttributeDto>(entity);
        }

        public async Task<bool> DeleteAsync(int detailId)
        {
            var entity = await _uow.OutfitAttributes.GetByIdAsync(detailId);
            if (entity == null) return false;

            await _uow.OutfitAttributes.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
