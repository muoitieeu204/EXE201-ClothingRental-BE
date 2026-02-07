using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.ServiceBookingDTOs;
using EXE201.Service.Interface;

namespace EXE201.Service.Implementation
{
    public class ServiceBookingService : IServiceBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceBookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
  _unitOfWork = unitOfWork;
            _mapper = mapper;
  }

        public async Task<IEnumerable<ServiceBookingResponseDto>> GetAllAsync()
        {
    var bookings = await _unitOfWork.ServiceBookings.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceBookingResponseDto>>(bookings);
        }

        public async Task<ServiceBookingDetailDto?> GetByIdAsync(int id)
        {
        var booking = await _unitOfWork.ServiceBookings.GetByIdAsync(id);
            if (booking == null) return null;
     return _mapper.Map<ServiceBookingDetailDto>(booking);
        }

        public async Task<IEnumerable<ServiceBookingResponseDto>> GetBookingsByUserIdAsync(int userId)
        {
        var bookings = await _unitOfWork.ServiceBookings.GetServiceBookingsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ServiceBookingResponseDto>>(bookings);
        }

        public async Task<IEnumerable<ServiceBookingResponseDto>> GetBookingsByBookingIdAsync(int bookingId)
        {
        var bookings = await _unitOfWork.ServiceBookings.GetServiceBookingsByBookingIdAsync(bookingId);
        return _mapper.Map<IEnumerable<ServiceBookingResponseDto>>(bookings);
        }

        public async Task<ServiceBookingResponseDto?> CreateAsync(CreateServiceBookingDto dto)
        {
    // Validate user exists
         var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
 if (user == null)
   return null;

       // Validate service package exists
            var servicePackage = await _unitOfWork.ServicePackages.GetByIdAsync(dto.ServicePkgId);
    if (servicePackage == null)
  return null;

            // Validate booking exists if provided
            if (dto.BookingId.HasValue)
     {
         var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId.Value);
 if (booking == null)
    return null;
       }

            var serviceBooking = _mapper.Map<ServiceBooking>(dto);
    
            // Set default status if not provided
            if (string.IsNullOrEmpty(serviceBooking.Status))
{
      serviceBooking.Status = "Pending";
    }

            await _unitOfWork.ServiceBookings.AddAsync(serviceBooking);
      await _unitOfWork.SaveChangesAsync();

      // Reload with navigation properties
            var createdBooking = await _unitOfWork.ServiceBookings.GetByIdAsync(serviceBooking.SvcBookingId);
            return _mapper.Map<ServiceBookingResponseDto>(createdBooking);
        }

   public async Task<bool> UpdateAsync(int id, UpdateServiceBookingDto dto)
  {
       var serviceBooking = await _unitOfWork.ServiceBookings.GetByIdAsync(id);
         if (serviceBooking == null)
            return false;

    // Validate user if being updated
            if (dto.UserId.HasValue && dto.UserId.Value != serviceBooking.UserId)
         {
       var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId.Value);
                if (user == null)
   return false;
            }

            // Validate service package if being updated
            if (dto.ServicePkgId.HasValue && dto.ServicePkgId.Value != serviceBooking.ServicePkgId)
   {
       var servicePackage = await _unitOfWork.ServicePackages.GetByIdAsync(dto.ServicePkgId.Value);
       if (servicePackage == null)
           return false;
            }

   // Validate booking if being updated
      if (dto.BookingId.HasValue)
         {
             var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId.Value);
              if (booking == null)
   return false;
      }

            _mapper.Map(dto, serviceBooking);
            await _unitOfWork.ServiceBookings.UpdateAsync(serviceBooking);
            await _unitOfWork.SaveChangesAsync();

    return true;
        }

        public async Task<bool> DeleteAsync(int id)
   {
            var serviceBooking = await _unitOfWork.ServiceBookings.GetByIdAsync(id);
            if (serviceBooking == null)
       return false;

 await _unitOfWork.ServiceBookings.DeleteAsync(serviceBooking);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
   var serviceBooking = await _unitOfWork.ServiceBookings.GetByIdAsync(id);
   return serviceBooking != null;
        }
    }
}
