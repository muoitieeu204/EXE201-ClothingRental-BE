using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.OutfitSizeDTOs;
using EXE201.Service.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class OutfitSizeService : IOutfitSizeService
    {
        private readonly IUnitOfWork _unitOfWork;
     private readonly IMapper _mapper;

        public OutfitSizeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
       _unitOfWork = unitOfWork;
      _mapper = mapper;
        }

        public async Task<IEnumerable<OutfitSizeResponseDto>> GetSizesByOutfitIdAsync(int outfitId)
        {
            var sizes = await _unitOfWork.OutfitSizes.GetSizesByOutfitIdAsync(outfitId);
            return _mapper.Map<IEnumerable<OutfitSizeResponseDto>>(sizes);
      }

    public async Task<IEnumerable<OutfitSizeResponseDto>> GetAvailableSizesByOutfitIdAsync(int outfitId)
        {
var sizes = await _unitOfWork.OutfitSizes.GetAvailableSizesByOutfitIdAsync(outfitId);
    return _mapper.Map<IEnumerable<OutfitSizeResponseDto>>(sizes);
     }

        public async Task<OutfitSizeResponseDto?> GetByIdAsync(int sizeId)
        {
            var size = await _unitOfWork.OutfitSizes.GetByIdAsync(sizeId);
            if (size == null) return null;
      return _mapper.Map<OutfitSizeResponseDto>(size);
        }

        public async Task<bool> AddAsync(CreateOutfitSizeDto dto)
        {
     // Verify outfit exists
      var outfitExists = await _unitOfWork.Outfits.ExistAsync(o => o.OutfitId == dto.OutfitId);
   if (!outfitExists) return false;

            var outfitSize = _mapper.Map<OutfitSize>(dto);
   await _unitOfWork.OutfitSizes.AddAsync(outfitSize);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

  public async Task<bool> UpdateAsync(int sizeId, UpdateOutfitSizeDto dto)
        {
    var outfitSize = await _unitOfWork.OutfitSizes.GetByIdAsync(sizeId);
            if (outfitSize == null) return false;

   _mapper.Map(dto, outfitSize);
        await _unitOfWork.OutfitSizes.UpdateAsync(outfitSize);
            await _unitOfWork.SaveChangesAsync();
   return true;
        }

   public async Task<bool> DeleteAsync(int sizeId)
        {
      var outfitSize = await _unitOfWork.OutfitSizes.GetByIdAsync(sizeId);
            if (outfitSize == null) return false;

    await _unitOfWork.OutfitSizes.DeleteAsync(outfitSize);
            await _unitOfWork.SaveChangesAsync();
 return true;
        }
    }
}
