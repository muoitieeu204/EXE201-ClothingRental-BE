using EXE201.Service.DTOs.StudioDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IStudioService
    {
        Task<IEnumerable<StudioDto>> GetAllIsActiveAsync();
        Task<StudioDto?> GetByIdAsync(int id);             
        Task<StudioDto?> CreateAsync(CreateStudioDto dto); 
        Task<StudioDto?> UpdateAsync(int id, UpdateStudioDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
    }

}
