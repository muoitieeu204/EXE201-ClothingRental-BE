using EXE201.Service.DTOs.ReviewImageDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IReviewImageService
    {
        Task<IEnumerable<ReviewImageResponseDto>> GetByReviewIdAsync(int reviewId);
        Task<ReviewImageResponseDto?> GetByIdAsync(int imgId);
        Task<ReviewImageResponseDto?> AddAsync(CreateReviewImageDto dto);
        Task<ReviewImageResponseDto?> UpdateAsync(int imgId, UpdateReviewImageDto dto);
        Task<bool> DeleteAsync(int imgId);
    }
}
