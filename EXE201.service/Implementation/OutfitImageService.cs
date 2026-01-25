using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.OutfitImageDTOs;
using EXE201.Service.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class OutfitImageService : IOutfitImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public OutfitImageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> AddAsync(CreateOutfitImageDto entity)
        {
            var exist = await IsInOutfit(entity.OutfitId);
            if (exist) return false;

            var outfitImage = _mapper.Map<OutfitImage>(entity);

            await _unitOfWork.OutfitImages.AddAsync(outfitImage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var outfitImage = await _unitOfWork.OutfitImages.GetByIdAsync(id);
            if (outfitImage == null) return false;
            await _unitOfWork.OutfitImages.DeleteAsync(outfitImage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<OutfitImageResponseDto?> GetByImageIdAsync(int id)
        {
            var outfitImage = await _unitOfWork.OutfitImages.GetByIdAsync(id);
            if (outfitImage == null) return null;
            return _mapper.Map<OutfitImageResponseDto>(outfitImage);
        }

        public async Task<IEnumerable<OutfitImageResponseDto>> GetImageByOutfitIdAsync(int id)
        {
            var outfitImage = await _unitOfWork.OutfitImages.GetImagesByOutfitIdAsync(id);
            return _mapper.Map<IEnumerable<OutfitImageResponseDto>>(outfitImage);
        }


        public async Task<bool> UpdateAsync(int id, UpdateOutfitImageDto entity)
        {
            var outfitImage = await _unitOfWork.OutfitImages.GetByIdAsync(id);
            if(outfitImage == null) return false;
            _mapper.Map(entity, outfitImage);
            await _unitOfWork.OutfitImages.UpdateAsync(outfitImage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsInOutfit(int outfitId)
        {
            return await _unitOfWork.OutfitImages.ExistAsync(o =>o.OutfitId == outfitId);
        }
    }
}
