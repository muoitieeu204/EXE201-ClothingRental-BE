using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.ReviewImageDTOs;
using EXE201.Service.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class ReviewImageService : IReviewImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewImageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReviewImageResponseDto>> GetByReviewIdAsync(int reviewId)
        {
            var images = await _unitOfWork.ReviewImages.GetImagesByReviewIdAsync(reviewId);
            return _mapper.Map<IEnumerable<ReviewImageResponseDto>>(images);
        }

        public async Task<ReviewImageResponseDto?> GetByIdAsync(int imgId)
        {
            var image = await _unitOfWork.ReviewImages.GetByIdAsync(imgId);
            return image == null ? null : _mapper.Map<ReviewImageResponseDto>(image);
        }

        public async Task<ReviewImageResponseDto?> AddAsync(CreateReviewImageDto dto)
        {
            var reviewExists = await _unitOfWork.Reviews.ExistAsync(r => r.ReviewId == dto.ReviewId);
            if (!reviewExists) return null;

            var image = _mapper.Map<ReviewImage>(dto);
            await _unitOfWork.ReviewImages.AddAsync(image);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReviewImageResponseDto>(image);
        }

        public async Task<ReviewImageResponseDto?> UpdateAsync(int imgId, UpdateReviewImageDto dto)
        {
            var image = await _unitOfWork.ReviewImages.GetByIdAsync(imgId);
            if (image == null) return null;

            _mapper.Map(dto, image);
            await _unitOfWork.ReviewImages.UpdateAsync(image);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReviewImageResponseDto>(image);
        }

        public async Task<bool> DeleteAsync(int imgId)
        {
            var image = await _unitOfWork.ReviewImages.GetByIdAsync(imgId);
            if (image == null) return false;

            await _unitOfWork.ReviewImages.DeleteAsync(image);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
