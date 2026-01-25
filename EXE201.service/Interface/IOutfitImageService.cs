using EXE201.Service.DTOs.OutfitImageDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IOutfitImageService
    {
        Task<IEnumerable<OutfitImageResponseDto>> GetImageByOutfitIdAsync(int id);
        Task<OutfitImageResponseDto?> GetByImageIdAsync(int id);
        Task<bool> AddAsync(CreateOutfitImageDto entity);
        Task<bool> UpdateAsync(int id, UpdateOutfitImageDto entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsInOutfit(int outfitId);
    }
}
