using EXE201.Service.DTOs.BookingDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IBookingService
    {
        //Task<IEnumerable<BookingDto>> GetMyBookingsAsync(int userId, bool includeDetails = false);
        //Task<BookingDto?> GetMyBookingByIdAsync(int userId, int bookingId);

        Task<BookingDto?> CreateMyBookingAsync(int userId, CreateBookingDto dto);

        Task<bool> CompleteMyBookingAsync(int userId, int bookingId);
        Task<bool> CancelMyBookingAsync(int userId, int bookingId);

        Task<IEnumerable<BookingDto>> GetMyBookingsAsync(int userId, bool includeDetails = false, bool includeServices = false);

        Task<BookingDto?> GetMyBookingByIdAsync(int userId, int bookingId, bool includeDetails = true, bool includeServices = true);

    }
}
