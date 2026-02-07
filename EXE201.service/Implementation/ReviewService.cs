using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.ReviewDTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetAllAsync(int? outfitId = null, int? userId = null)
        {
            IEnumerable<Review> reviews;

            if (outfitId.HasValue && userId.HasValue)
            {
                reviews = (await _unitOfWork.Reviews.GetReviewsByOutfitIdAsync(outfitId.Value))
                    .Where(r => r.UserId == userId.Value)
                    .ToList();
            }
            else if (outfitId.HasValue)
            {
                reviews = await _unitOfWork.Reviews.GetReviewsByOutfitIdAsync(outfitId.Value);
            }
            else if (userId.HasValue)
            {
                reviews = await _unitOfWork.Reviews.GetReviewsByUserIdAsync(userId.Value);
            }
            else
            {
                reviews = await _unitOfWork.Reviews.GetAllAsync();
            }

            return _mapper.Map<IEnumerable<ReviewResponseDto>>(reviews);
        }

        public async Task<ReviewResponseDto?> GetByIdAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            return review == null ? null : _mapper.Map<ReviewResponseDto>(review);
        }

        public async Task<ReviewResponseDto?> AddAsync(CreateReviewDto dto)
        {
            var outfitExists = await _unitOfWork.Outfits.ExistAsync(o => o.OutfitId == dto.OutfitId);
            if (!outfitExists) return null;

            var userExists = await _unitOfWork.Users.ExistAsync(u => u.UserId == dto.UserId);
            if (!userExists) return null;

            var review = _mapper.Map<Review>(dto);
            review.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            var images = BuildReviewImages(review.ReviewId, dto.ImageUrls);
            if (images.Count > 0)
            {
                foreach (var image in images)
                {
                    await _unitOfWork.ReviewImages.AddAsync(image);
                }

                await _unitOfWork.SaveChangesAsync();
                review.ReviewImages = images;
            }

            return _mapper.Map<ReviewResponseDto>(review);
        }

        public async Task<ReviewResponseDto?> UpdateAsync(int reviewId, UpdateReviewDto dto)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null) return null;

            _mapper.Map(dto, review);

            if (dto.ImageUrls != null)
            {
                var existingImages = await _unitOfWork.ReviewImages.GetImagesByReviewIdAsync(reviewId);
                foreach (var image in existingImages)
                {
                    await _unitOfWork.ReviewImages.DeleteAsync(image);
                }

                var newImages = BuildReviewImages(reviewId, dto.ImageUrls);
                foreach (var image in newImages)
                {
                    await _unitOfWork.ReviewImages.AddAsync(image);
                }

                review.ReviewImages = newImages;
            }

            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReviewResponseDto>(review);
        }

        public async Task<bool> DeleteAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null) return false;

            var images = await _unitOfWork.ReviewImages.GetImagesByReviewIdAsync(reviewId);
            foreach (var image in images)
            {
                await _unitOfWork.ReviewImages.DeleteAsync(image);
            }

            await _unitOfWork.Reviews.DeleteAsync(review);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static List<ReviewImage> BuildReviewImages(int reviewId, IEnumerable<string>? imageUrls)
        {
            if (imageUrls == null) return new List<ReviewImage>();

            return imageUrls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => new ReviewImage
                {
                    ReviewId = reviewId,
                    ImageUrl = url.Trim()
                })
                .ToList();
        }
    }
}
