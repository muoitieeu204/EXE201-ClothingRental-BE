using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<bool> AddAsync(OutfitImageDTO entity)
        {
            var exist = await IsInOutfit(entity.ImageId, entity.OutfitId);
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

        public async Task<OutfitImageDTO> GetByImageIdAsync(int id)
        {
            var outfitImage = await _unitOfWork.OutfitImages.GetByIdAsync(id);
            if (outfitImage == null) return null;
            return _mapper.Map<OutfitImageDTO>(outfitImage);
        }

        public async Task<IEnumerable<OutfitImageDTO>> GetImageByOutfitIdAsync(int id)
        {
            var outfitImage = await _unitOfWork.OutfitImages.GetImagesByOutfitIdAsync(id);
            return _mapper.Map<IEnumerable<OutfitImageDTO>>(outfitImage);
        }


        public async Task<bool> UpdateAsync(int id, OutfitImageDTO entity)
        {
            var outfitImage = await _unitOfWork.OutfitImages.GetByIdAsync(id);
            if(outfitImage == null) return false;
            _mapper.Map(entity, outfitImage);
            await _unitOfWork.OutfitImages.UpdateAsync(outfitImage);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsInOutfit(int id, int outfitId)
        {
            return await _unitOfWork.OutfitImages.ExistAsync(o => o.ImageId == id && o.OutfitId == outfitId);
        }
    }
}
