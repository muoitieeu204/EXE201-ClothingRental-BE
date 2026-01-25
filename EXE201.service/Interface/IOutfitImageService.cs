using EXE201.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IOutfitImageService
    {
        Task<IEnumerable<OutfitImageDTO>> GetImageByOutfitIdAsync(int id);
        Task<OutfitImageDTO> GetByImageIdAsync(int id);
        Task<bool> AddAsync(OutfitImageDTO entity);
        Task<bool> UpdateAsync(int id, OutfitImageDTO entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsInOutfit(int id, int outfitId);

    }
}
