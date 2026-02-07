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
            if (dto.AddressId <= 0) return null;

            // 1) Validate UserAddress + snapshot AddressText
            var address = await _uow.UserAddresses.GetByIdAsync(dto.AddressId); // << đổi từ Addresses sang UserAddresses
            if (address == null) return null;
            if (address.UserId != userId) return null;

            // bắt buộc phải có AddressLine (vì table cho NULL nhưng đặt hàng mà thiếu thì chịu)
            if (string.IsNullOrWhiteSpace(address.AddressLine))
                return null;

            var addressSnapshot = BuildAddressSnapshot(address);

            // 2) Pre-validate items + build details in-memory (chưa ghi DB)
            var details = new List<BookingDetail>();
            decimal totalRental = 0m;
            decimal totalDeposit = 0m;
            decimal totalSurcharge = 0m;

            foreach (var item in dto.Items)
            {
                var size = await _uow.OutfitSizes.GetByIdAsync(item.OutfitSizeId);
                if (size == null) return null;

                var outfit = await _uow.Outfits.GetByIdAsync(size.OutfitId);
                if (outfit == null) return null;

                var pkg = await _uow.RentalPackages.GetByIdAsync(item.RentalPackageId);
                if (pkg == null) return null;

                var start = item.StartTime;
                if (start == default) return null;

                var end = start.AddHours(pkg.DurationHours);

                var unitPrice = outfit.BaseRentalPrice * (decimal)pkg.PriceFactor;

                var depositPercent = (decimal)(pkg.DepositPercent ?? 0);
                if (depositPercent < 0) depositPercent = 0;
                // nếu data bị nhập 30 thay vì 0.3 thì chặn nhẹ
                if (depositPercent > 1) depositPercent = depositPercent / 100m;

                var deposit = unitPrice * depositPercent;

                if (pkg.WeekendSurchargePercent.HasValue)
                {
                    var isWeekend = start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday;
                    if (isWeekend)
                        totalSurcharge += unitPrice * (decimal)pkg.WeekendSurchargePercent.Value;
                }

                details.Add(new BookingDetail
                {
                    OutfitSizeId = item.OutfitSizeId,
                    RentalPackageId = item.RentalPackageId,
                    StartTime = start,
                    EndTime = end,
                    UnitPrice = unitPrice,
                    DepositAmount = deposit,
                    LateFee = 0,
                    DamageFee = 0,
                    Status = "Pending"
                });

                totalRental += unitPrice;
                totalDeposit += deposit;
            }

            // 3) Create booking (set totals trước)
            var booking = new Booking
            {
                UserId = userId,
                AddressId = address.AddressId,
                AddressText = addressSnapshot, // snapshot từ UserAddress
                TotalRentalAmount = totalRental,
                TotalDepositAmount = totalDeposit,
                TotalSurcharge = totalSurcharge,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                BookingDate = DateTime.Now
            };

            // 4) Save booking để lấy BookingId
            await _uow.Bookings.AddAsync(booking);
            await _uow.SaveChangesAsync();

            // 5) Save details
            foreach (var d in details)
            {
                d.BookingId = booking.BookingId;
                await _uow.BookingDetails.AddAsync(d);
            }
            await _uow.SaveChangesAsync();

            // 6) Return created booking
            return await GetMyBookingByIdAsync(userId, booking.BookingId);
        }

        private static string BuildAddressSnapshot(UserAddress a)
        {
            // Format: "RecipientName - Phone, AddressLine, Ward, District, City"
            var headParts = new List<string>();

            var name = a.RecipientName?.Trim();
            var phone = a.PhoneNumber?.Trim();

            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(phone))
                headParts.Add($"{name} - {phone}");
            else if (!string.IsNullOrWhiteSpace(name))
                headParts.Add(name);
            else if (!string.IsNullOrWhiteSpace(phone))
                headParts.Add(phone);

            var tailParts = new List<string?>
    {
        a.AddressLine,
        a.Ward,
        a.District,
        a.City
    };

            var tail = string.Join(", ", tailParts
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim()));

            if (headParts.Count == 0) return tail;
            if (string.IsNullOrWhiteSpace(tail)) return headParts[0];

            return $"{headParts[0]}, {tail}";
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
