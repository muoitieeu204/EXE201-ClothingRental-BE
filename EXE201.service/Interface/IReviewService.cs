using EXE201.Service.DTOs.ReviewDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewResponseDto>> GetAllAsync(int? outfitId = null, int? userId = null);
        Task<ReviewResponseDto?> GetByIdAsync(int reviewId);
        Task<ReviewResponseDto?> AddAsync(CreateReviewDto dto);
        Task<ReviewResponseDto?> UpdateAsync(int reviewId, UpdateReviewDto dto);
        Task<bool> DeleteAsync(int reviewId);
    }
}
