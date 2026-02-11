using EXE201.Service.DTOs.ServiceBookingDTOs;

namespace EXE201.Service.Interface
{
    public interface IServiceBookingService
    {
  Task<IEnumerable<ServiceBookingResponseDto>> GetAllAsync();
        Task<ServiceBookingDetailDto?> GetByIdAsync(int id);
        Task<IEnumerable<ServiceBookingResponseDto>> GetBookingsByUserIdAsync(int userId);
   Task<IEnumerable<ServiceBookingResponseDto>> GetBookingsByBookingIdAsync(int bookingId);
  Task<ServiceBookingResponseDto?> CreateAsync(CreateServiceBookingDto dto);
        Task<bool> UpdateAsync(int id, UpdateServiceBookingDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
