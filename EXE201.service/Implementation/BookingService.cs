using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.BookingDTOs;
using EXE201.Service.DTOs.ServiceBookingDTOs;
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

        public async Task<IEnumerable<BookingDto>> GetMyBookingsAsync(int userId, bool includeDetails = false, bool includeServices = false)
        {
            var bookings = await _uow.Bookings.GetBookingsByUserIdAsync(userId);
            var result = _mapper.Map<List<BookingDto>>(bookings);

            if (!includeDetails && !includeServices) return result;

            foreach (var b in result)
            {
                if (includeDetails)
                {
                    var details = await _uow.BookingDetails.FindAsync(d => d.BookingId == b.BookingId);
                    b.Details = _mapper.Map<List<BookingDetailDto>>(details);
                }

                if (includeServices)
                {
                    var services = await _uow.ServiceBookings.FindAsync(s => s.BookingId == b.BookingId);
                    b.Services = _mapper.Map<List<ServiceBookingDto>>(services);

                    foreach (var s in b.Services)
                    {
                        var addons = await _uow.ServiceBookingAddons.FindAsync(a => a.SvcBookingId == s.SvcBookingId);
                        s.Addons = _mapper.Map<List<ServiceBookingAddonDto>>(addons);
                    }
                }
            }

            return result;
        }


        public async Task<BookingDto?> GetMyBookingByIdAsync(int userId, int bookingId, bool includeDetails = true, bool includeServices = true)
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId);
            if (booking == null || booking.UserId != userId) return null;

            var dto = _mapper.Map<BookingDto>(booking);

            if (includeDetails)
            {
                var details = await _uow.BookingDetails.FindAsync(d => d.BookingId == bookingId);
                dto.Details = _mapper.Map<List<BookingDetailDto>>(details);
            }

            if (includeServices)
            {
                var services = await _uow.ServiceBookings.FindAsync(s => s.BookingId == bookingId);
                dto.Services = _mapper.Map<List<ServiceBookingDto>>(services);

                foreach (var s in dto.Services)
                {
                    var addons = await _uow.ServiceBookingAddons.FindAsync(a => a.SvcBookingId == s.SvcBookingId);
                    s.Addons = _mapper.Map<List<ServiceBookingAddonDto>>(addons);
                }
            }

            return dto;
        }


        public async Task<BookingDto?> CreateMyBookingAsync(int userId, CreateBookingDto dto)
        {
            if (dto == null) return null;
            if (dto.AddressId <= 0) return null;
            if (dto.Items == null || dto.Items.Count == 0) return null;

            // 1) validate address belongs to user
            var address = await _uow.UserAddresses.GetByIdAsync(dto.AddressId);
            if (address == null) return null;
            if (address.UserId != userId) return null;
            if (string.IsNullOrWhiteSpace(address.AddressLine)) return null;

            var addressSnapshot = BuildAddressSnapshot(address);

            // 2) build details (KHÔNG tính toán)
            var details = new List<BookingDetail>();
            decimal totalRental = 0m, totalDeposit = 0m, totalSurcharge = 0m;

            foreach (var item in bookingItems)
            {
                if (item.OutfitSizeId <= 0) return null;
                if (item.StartTime == default) return null;

                // validate size tồn tại (đỡ FK chết)
                var size = await _uow.OutfitSizes.GetByIdAsync(item.OutfitSizeId);
                if (size == null) return null;

                // normalize: 0 hoặc <=0 => null
                if (item.RentalPackageId.HasValue && item.RentalPackageId.Value <= 0)
                    item.RentalPackageId = null;

                // nếu có packageId thì validate tồn tại (đỡ 500 do FK)
                if (item.RentalPackageId.HasValue)
                {
                    var pkg = await _uow.RentalPackages.GetByIdAsync(item.RentalPackageId.Value);
                    if (pkg == null) return null;
                }

                if (item.UnitPrice < 0 || item.DepositAmount < 0 || item.Surcharge < 0)
                    return null;

                details.Add(new BookingDetail
                {
                    OutfitSizeId = item.OutfitSizeId,
                    RentalPackageId = item.RentalPackageId, // null ok

                    StartTime = item.StartTime,
                    EndTime = item.EndTime,                 // null ok

                    UnitPrice = item.UnitPrice,
                    DepositAmount = item.DepositAmount,
                    LateFee = 0,
                    DamageFee = 0,
                    Status = "Pending"
                });

                totalRental += item.UnitPrice;
                totalDeposit += item.DepositAmount;
                totalSurcharge += item.Surcharge;
            }

            // 3) create booking
            var booking = new Booking
            {
                UserId = userId,
                AddressId = address.AddressId,
                AddressText = addressSnapshot,

                TotalRentalAmount = totalRental,
                TotalDepositAmount = totalDeposit,
                TotalSurcharge = totalSurcharge,

                Status = "Pending",
                PaymentStatus = "Unpaid",
                BookingDate = DateTime.Now
            };

            await _uow.Bookings.AddAsync(booking);
            await _uow.SaveChangesAsync();

            foreach (var d in details)
            {
                d.BookingId = booking.BookingId;
                await _uow.BookingDetails.AddAsync(d);
            }
            await _uow.SaveChangesAsync();

            // 4) optional: create ServiceBooking (CHỈ khi có ServicePkgId)
            if (dto.Service != null)
            {
                // normalize 0 => null
                if (dto.Service.ServicePkgId.HasValue && dto.Service.ServicePkgId.Value <= 0)
                    dto.Service.ServicePkgId = null;

                if (dto.Service.ServicePkgId.HasValue)
                {
                    // validate service package tồn tại (đỡ 500 FK)
                    var sp = await _uow.ServicePackages.GetByIdAsync(dto.Service.ServicePkgId.Value);
                    if (sp == null) return null;

                    // serviceTime bắt buộc khi có service
                    if (!dto.Service.ServiceTime.HasValue || dto.Service.ServiceTime.Value == default)
                        return null;

                    var svcBooking = new ServiceBooking
                    {
                        UserId = userId,
                        BookingId = booking.BookingId,
                        ServicePkgId = dto.Service.ServicePkgId.Value,
                        ServiceTime = dto.Service.ServiceTime.Value,
                        TotalPrice = dto.Service.TotalPrice, // client gửi
                        Status = "Pending"
                    };

                    await _uow.ServiceBookings.AddAsync(svcBooking);
                    await _uow.SaveChangesAsync(); // lấy SvcBookingId

                    if (dto.Service.Addons != null && dto.Service.Addons.Count > 0)
                    {
                        foreach (var a in dto.Service.Addons)
                        {
                            var addon = await _uow.ServiceAddons.GetByIdAsync(a.AddonId);
                            if (addon == null) return null;

                            await _uow.ServiceBookingAddons.AddAsync(new ServiceBookingAddon
                            {
                                SvcBookingId = svcBooking.SvcBookingId,
                                AddonId = a.AddonId,
                                PriceAtBooking = a.PriceAtBooking
                            });
                        }
                        await _uow.SaveChangesAsync();
                    }
                }
                //nếu ServicePkgId null => không tạo record service
            }

            return await GetMyBookingByIdAsync(userId, booking.BookingId, includeDetails: true, includeServices: true);
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

            var serviceBookings = await _uow.ServiceBookings.GetServiceBookingsByBookingIdAsync(bookingId);
            foreach (var sb in serviceBookings)
            {
                sb.Status = "Completed";
                await _uow.ServiceBookings.UpdateAsync(sb);
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

            var serviceBookings = await _uow.ServiceBookings.GetServiceBookingsByBookingIdAsync(bookingId);
            foreach (var sb in serviceBookings)
            {
                sb.Status = "Cancelled";
                await _uow.ServiceBookings.UpdateAsync(sb);
            }

            await _uow.Bookings.UpdateAsync(booking);
            await _uow.SaveChangesAsync();
            return true;
        }

    }

}
