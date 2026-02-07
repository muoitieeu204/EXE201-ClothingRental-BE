using EXE201.Service.DTOs.OutfitAttributeDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IOutfitAttributeService
    {
        Task<IEnumerable<OutfitAttributeDto>> GetAllAsync();
        Task<OutfitAttributeDto?> GetByIdAsync(int detailId);
        Task<OutfitAttributeDto?> GetByOutfitIdAsync(int outfitId);

        Task<OutfitAttributeDto?> CreateAsync(CreateOutfitAttributeDto dto);
        Task<OutfitAttributeDto?> UpdateAsync(int detailId, UpdateOutfitAttributeDto dto);
        Task<bool> DeleteAsync(int detailId);
    }
}
