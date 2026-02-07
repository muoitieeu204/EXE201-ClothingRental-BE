using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.OutfitDTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class OutfitService : IOutfitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OutfitService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
  _mapper = mapper;
        }

        public async Task<IEnumerable<OutfitResponseDto>> GetAllAsync()
        {
  var outfits = await _unitOfWork.Outfits.GetAllAsync();
     return _mapper.Map<IEnumerable<OutfitResponseDto>>(outfits);
  }

        public async Task<IEnumerable<OutfitResponseDto>> GetAvailableOutfitsAsync()
        {
var outfits = await _unitOfWork.Outfits.GetAvailableOutfitsAsync();
 return _mapper.Map<IEnumerable<OutfitResponseDto>>(outfits);
        }

        public async Task<IEnumerable<OutfitResponseDto>> GetOutfitsByCategoryIdAsync(int categoryId)
  {
            var outfits = await _unitOfWork.Outfits.GetOutfitsByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<OutfitResponseDto>>(outfits);
      }

        public async Task<OutfitResponseDto?> GetByIdAsync(int outfitId)
        {
       var outfit = await _unitOfWork.Outfits.GetByIdAsync(outfitId);
         if (outfit == null) return null;
            return _mapper.Map<OutfitResponseDto>(outfit);
        }

        public async Task<OutfitDetailDto?> GetDetailByIdAsync(int outfitId)
      {
  var outfit = await _unitOfWork.Outfits.GetByIdAsync(outfitId);
     if (outfit == null) return null;
   return _mapper.Map<OutfitDetailDto>(outfit);
  }

        public async Task<IEnumerable<OutfitResponseDto>> SearchAsync(string searchTerm)
        {
            var outfits = await _unitOfWork.Outfits.FindAsync(o => 
      o.Name.Contains(searchTerm) || 
                (o.Type != null && o.Type.Contains(searchTerm)) ||
    (o.Gender != null && o.Gender.Contains(searchTerm)) ||
           (o.Region != null && o.Region.Contains(searchTerm))
            );
          return _mapper.Map<IEnumerable<OutfitResponseDto>>(outfits);
        }

        public async Task<int> AddAsync(CreateOutfitDto dto)
        {
       // Verify category exists
    var categoryExists = await _unitOfWork.Categories.ExistAsync(c => c.CategoryId == dto.CategoryId);
       if (!categoryExists) return 0;

    var outfit = _mapper.Map<Outfit>(dto);
       outfit.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Outfits.AddAsync(outfit);
await _unitOfWork.SaveChangesAsync();
 
            return outfit.OutfitId;
    }

        public async Task<bool> UpdateAsync(int outfitId, UpdateOutfitDto dto)
   {
       var outfit = await _unitOfWork.Outfits.GetByIdAsync(outfitId);
     if (outfit == null) return false;

     // Verify category exists if changing category
      if (dto.CategoryId.HasValue)
         {
 var categoryExists = await _unitOfWork.Categories.ExistAsync(c => c.CategoryId == dto.CategoryId.Value);
   if (!categoryExists) return false;
            }

  _mapper.Map(dto, outfit);
            await _unitOfWork.Outfits.UpdateAsync(outfit);
            await _unitOfWork.SaveChangesAsync();
      return true;
  }

        public async Task<bool> DeleteAsync(int outfitId)
        {
 var outfit = await _unitOfWork.Outfits.GetByIdAsync(outfitId);
 if (outfit == null) return false;

      await _unitOfWork.Outfits.DeleteAsync(outfit);
     await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
