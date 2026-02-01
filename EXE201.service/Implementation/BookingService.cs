using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.BookingDTOs;
using EXE201.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public BookingService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        private static bool IsActive(string? s)
            => !string.IsNullOrWhiteSpace(s) && s.Trim().Equals("Active", StringComparison.OrdinalIgnoreCase);

        public async Task<IEnumerable<BookingDto>> GetMyBookingsAsync(int userId, bool includeDetails = false)
        {
            var bookings = await _uow.Bookings.GetBookingsByUserIdAsync(userId);
            var result = _mapper.Map<List<BookingDto>>(bookings);

            if (!includeDetails) return result;

            // Không sửa repo => load details kiểu N+1 cho dễ test
            foreach (var b in result)
            {
                var details = await _uow.BookingDetails.FindAsync(d => d.BookingId == b.BookingId);
                b.Details = _mapper.Map<List<BookingDetailDto>>(details);
            }

            return result;
        }

        public async Task<BookingDto?> GetMyBookingByIdAsync(int userId, int bookingId)
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId);
            if (booking == null || booking.UserId != userId) return null;

            var dto = _mapper.Map<BookingDto>(booking);
            var details = await _uow.BookingDetails.FindAsync(d => d.BookingId == bookingId);
            dto.Details = _mapper.Map<List<BookingDetailDto>>(details);

            return dto;
        }

        public async Task<BookingDto?> CreateMyBookingAsync(int userId, CreateBookingDto dto)
        {
            if (dto == null) return null;
            if (dto.Items == null || dto.Items.Count == 0) return null;

            // 1) Validate AddressId + snapshot AddressText
            var address = await _uow.Addresses.GetByIdAsync(dto.AddressId);
            if (address == null) return null;
            if (address.UserId != userId) return null;

            // Chỉ cho dùng Address Active
            if (string.IsNullOrWhiteSpace(address.Status) ||
                !address.Status.Trim().Equals("Active", StringComparison.OrdinalIgnoreCase))
                return null;

            var booking = new Booking
            {
                UserId = userId,
                AddressId = address.AddressId,
                AddressText = address.AddressText, // snapshot
                Status = "Pending",
                PaymentStatus = "Unpaid",
                BookingDate = DateTime.Now
            };

            // 2) Add booking trước để lấy BookingId
            await _uow.Bookings.AddAsync(booking);
            await _uow.SaveChangesAsync();

            decimal totalRental = 0m;
            decimal totalDeposit = 0m;
            decimal totalSurcharge = 0m;

            // 3) Create BookingDetails + tính tổng
            foreach (var item in dto.Items)
            {
                var size = await _uow.OutfitSizes.GetByIdAsync(item.OutfitSizeId);
                if (size == null) return null;

                var outfit = await _uow.Outfits.GetByIdAsync(size.OutfitId);
                if (outfit == null) return null;

                var pkg = await _uow.RentalPackages.GetByIdAsync(item.RentalPackageId);
                if (pkg == null) return null;

                var unitPrice = outfit.BaseRentalPrice * (decimal)pkg.PriceFactor;

                var depositPercent = (decimal)(pkg.DepositPercent ?? 0);
                var deposit = unitPrice * depositPercent;

                var start = item.StartTime;
                var end = start.AddHours(pkg.DurationHours);

                // Optional: phụ thu cuối tuần (nếu có)
                if (pkg.WeekendSurchargePercent.HasValue)
                {
                    var isWeekend = start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday;
                    if (isWeekend)
                        totalSurcharge += unitPrice * (decimal)pkg.WeekendSurchargePercent.Value;
                }

                var detail = new BookingDetail
                {
                    BookingId = booking.BookingId,
                    OutfitSizeId = item.OutfitSizeId,
                    RentalPackageId = item.RentalPackageId,
                    StartTime = start,
                    EndTime = end,
                    UnitPrice = unitPrice,
                    DepositAmount = deposit,
                    LateFee = 0,
                    DamageFee = 0,
                    Status = "Pending"
                };

                await _uow.BookingDetails.AddAsync(detail);

                totalRental += unitPrice;
                totalDeposit += deposit;
            }

            // 4) Update totals
            booking.TotalRentalAmount = totalRental;
            booking.TotalDepositAmount = totalDeposit;
            booking.TotalSurcharge = totalSurcharge;

            await _uow.Bookings.UpdateAsync(booking);
            await _uow.SaveChangesAsync();

            // 5) Return created booking (kèm details nếu GetMyBookingByIdAsync có map details)
            return await GetMyBookingByIdAsync(userId, booking.BookingId);
        }

        public async Task<bool> CompleteMyBookingAsync(int userId, int bookingId)
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId);
            if (booking == null) return false;
            if (booking.UserId != userId) return false;

            if (string.IsNullOrWhiteSpace(booking.Status) ||
                !booking.Status.Trim().Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return false; // chỉ cho complete khi Pending

            booking.Status = "Completed";
            await _uow.Bookings.UpdateAsync(booking);

            var details = await _uow.BookingDetails.FindAsync(d => d.BookingId == bookingId);
            foreach (var d in details)
            {
                // đồng bộ detail
                d.Status = "Completed";
                await _uow.BookingDetails.UpdateAsync(d);
            }

            await _uow.SaveChangesAsync();
            return true;
        }



        public async Task<bool> CancelMyBookingAsync(int userId, int bookingId)
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId);
            if (booking == null || booking.UserId != userId) return false;

            // Nếu đã Completed thì không cho cancel (tuỳ bạn)
            if (!string.IsNullOrWhiteSpace(booking.Status) &&
                booking.Status.Trim().Equals("Completed", StringComparison.OrdinalIgnoreCase))
                return false;

            booking.Status = "Cancelled";

            // cancel luôn details
            var details = await _uow.BookingDetails.FindAsync(d => d.BookingId == bookingId);
            foreach (var d in details)
            {
                d.Status = "Cancelled";
                await _uow.BookingDetails.UpdateAsync(d);
            }

            await _uow.Bookings.UpdateAsync(booking);
            await _uow.SaveChangesAsync();
            return true;
        }

    }

}
