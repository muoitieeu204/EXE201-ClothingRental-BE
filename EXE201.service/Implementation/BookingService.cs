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

        public async Task<IEnumerable<BookingDto>> GetMyBookingsAsync(int userId, bool includeDetails = false)
        {
            var bookings = await _uow.Bookings.GetBookingsByUserIdAsync(userId);
            var result = _mapper.Map<List<BookingDto>>(bookings);

            foreach (var b in result)
            {
                if (includeDetails)
                {
                    var details = await _uow.BookingDetails.GetDetailsByBookingIdAsync(b.BookingId);
                    b.Details = await BuildBookingDetailDtosAsync(details);
                }

                await AttachComputedTotalsAsync(b);
            }

            return result;
        }

        public async Task<BookingDto?> GetMyBookingByIdAsync(int userId, int bookingId)
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId);
            if (booking == null || booking.UserId != userId) return null;

            var dto = _mapper.Map<BookingDto>(booking);
            var details = await _uow.BookingDetails.GetDetailsByBookingIdAsync(bookingId);
            dto.Details = await BuildBookingDetailDtosAsync(details);
            await AttachComputedTotalsAsync(dto);

            return dto;
        }

        public async Task<BookingDto?> CreateMyBookingAsync(int userId, CreateBookingDto dto)
        {
            if (dto == null) return null;
            if (dto.AddressId <= 0) return null;

            const decimal orderDepositRate = 0.3m;
            var rentalDays = dto.RentalDays <= 0 ? 1 : dto.RentalDays;

            var bookingItems = dto.Items ?? new List<CreateBookingItemDto>();
            var rawServicePackageIds = dto.ServicePackageIds ?? new List<int>();
            if (bookingItems.Count == 0 && rawServicePackageIds.Count == 0) return null;
            if (rawServicePackageIds.Any(id => id <= 0)) return null;

            var servicePackageIds = rawServicePackageIds
                .Distinct()
                .ToList();

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
            var serviceBookings = new List<ServiceBooking>();
            decimal totalRental = 0m;
            decimal totalService = 0m;

            foreach (var item in bookingItems)
            {
                var size = await _uow.OutfitSizes.GetByIdAsync(item.OutfitSizeId);
                if (size == null) return null;

                var outfit = await _uow.Outfits.GetByIdAsync(size.OutfitId);
                if (outfit == null) return null;

                var packageId = item.RentalPackageId ?? 0;
                RentalPackage? pkg = null;
                if (packageId > 0)
                {
                    pkg = await _uow.RentalPackages.GetByIdAsync(packageId);
                }
                else
                {
                    pkg = await GetFallbackRentalPackageAsync();
                }

                if (pkg == null) return null;

                var start = item.StartTime ?? DateTime.Now;

                var end = start.AddDays(rentalDays);
                var unitPrice = outfit.BaseRentalPrice * rentalDays;

                details.Add(new BookingDetail
                {
                    OutfitSizeId = item.OutfitSizeId,
                    RentalPackageId = pkg.PackageId,
                    StartTime = start,
                    EndTime = end,
                    UnitPrice = unitPrice,
                    DepositAmount = 0m,
                    LateFee = 0,
                    DamageFee = 0,
                    Status = "Pending"
                });

                totalRental += unitPrice;
            }

            foreach (var servicePackageId in servicePackageIds)
            {
                var servicePackage = await _uow.ServicePackages.GetByIdAsync(servicePackageId);
                if (servicePackage == null) return null;

                serviceBookings.Add(new ServiceBooking
                {
                    UserId = userId,
                    ServicePkgId = servicePackageId,
                    ServiceTime = null,
                    TotalPrice = servicePackage.BasePrice,
                    Status = "Pending"
                });

                totalService += servicePackage.BasePrice;
            }

            var totalOrderAmount = totalRental + totalService;
            var totalDeposit = Math.Round(totalOrderAmount * orderDepositRate, 0, MidpointRounding.AwayFromZero);

            // 3) Create booking (set totals trước)
            var booking = new Booking
            {
                UserId = userId,
                AddressId = address.AddressId,
                AddressText = addressSnapshot, // snapshot từ UserAddress
                TotalRentalAmount = totalRental,
                TotalDepositAmount = totalDeposit,
                TotalSurcharge = 0m,
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

            // 6) Save selected service packages into ServiceBookings
            foreach (var serviceBooking in serviceBookings)
            {
                serviceBooking.BookingId = booking.BookingId;
                await _uow.ServiceBookings.AddAsync(serviceBooking);
            }

            if (serviceBookings.Count > 0)
            {
                await _uow.SaveChangesAsync();
            }

            // 7) Return created booking with computed totals
            return await GetMyBookingByIdAsync(userId, booking.BookingId);
        }

        private async Task AttachComputedTotalsAsync(BookingDto dto)
        {
            var serviceBookings = (await _uow.ServiceBookings.GetServiceBookingsByBookingIdAsync(dto.BookingId)).ToList();
            dto.ServiceBookings = _mapper.Map<List<ServiceBookingResponseDto>>(serviceBookings);
            var totalService = dto.ServiceBookings.Sum(sb => sb.TotalPrice ?? 0m);
            dto.TotalServiceAmount = totalService;
            dto.TotalOrderAmount = (dto.TotalRentalAmount ?? 0m) + (dto.TotalSurcharge ?? 0m) + totalService;
        }

        private async Task<List<BookingDetailDto>> BuildBookingDetailDtosAsync(IEnumerable<BookingDetail> details)
        {
            var result = new List<BookingDetailDto>();

            var rentalPackageCache = new Dictionary<int, RentalPackage?>();
            var outfitSizeCache = new Dictionary<int, OutfitSize?>();
            var outfitCache = new Dictionary<int, Outfit?>();
            var outfitImageCache = new Dictionary<int, string?>();

            foreach (var detail in details.OrderBy(d => d.DetailId))
            {
                var detailDto = _mapper.Map<BookingDetailDto>(detail);
                detailDto.RentalDays = CalculateRentalDays(detail.StartTime, detail.EndTime);

                if (detail.RentalPackageId > 0)
                {
                    if (!rentalPackageCache.TryGetValue(detail.RentalPackageId, out var rentalPackage))
                    {
                        rentalPackage = await _uow.RentalPackages.GetByIdAsync(detail.RentalPackageId);
                        rentalPackageCache[detail.RentalPackageId] = rentalPackage;
                    }

                    detailDto.RentalPackageName = rentalPackage?.Name;
                }

                if (detail.OutfitSizeId > 0)
                {
                    if (!outfitSizeCache.TryGetValue(detail.OutfitSizeId, out var outfitSize))
                    {
                        outfitSize = await _uow.OutfitSizes.GetByIdAsync(detail.OutfitSizeId);
                        outfitSizeCache[detail.OutfitSizeId] = outfitSize;
                    }

                    if (outfitSize != null)
                    {
                        detailDto.OutfitId = outfitSize.OutfitId;
                        detailDto.OutfitSizeLabel = outfitSize.SizeLabel;

                        if (!outfitCache.TryGetValue(outfitSize.OutfitId, out var outfit))
                        {
                            outfit = await _uow.Outfits.GetByIdAsync(outfitSize.OutfitId);
                            outfitCache[outfitSize.OutfitId] = outfit;
                        }

                        if (outfit != null)
                        {
                            detailDto.OutfitName = outfit.Name;
                            detailDto.OutfitType = outfit.Type;
                        }

                        if (!outfitImageCache.TryGetValue(outfitSize.OutfitId, out var primaryImageUrl))
                        {
                            var outfitImages = (await _uow.OutfitImages.FindAsync(img => img.OutfitId == outfitSize.OutfitId)).ToList();
                            primaryImageUrl = outfitImages
                                .OrderBy(img => img.SortOrder ?? int.MaxValue)
                                .ThenBy(img => img.ImageId)
                                .Select(img => img.ImageUrl)
                                .FirstOrDefault();

                            outfitImageCache[outfitSize.OutfitId] = primaryImageUrl;
                        }

                        detailDto.OutfitImageUrl = primaryImageUrl;
                    }
                }

                result.Add(detailDto);
            }

            return result;
        }

        private static int? CalculateRentalDays(DateTime? startTime, DateTime? endTime)
        {
            if (!startTime.HasValue || !endTime.HasValue) return null;

            var dayDiff = (endTime.Value.Date - startTime.Value.Date).Days;
            return dayDiff <= 0 ? 1 : dayDiff;
        }

        // Keep DB constraint intact: if client does not send rentalPackageId, choose a base package automatically.
        private async Task<RentalPackage?> GetFallbackRentalPackageAsync()
        {
            var packages = (await _uow.RentalPackages.GetAllAsync())
                .ToList();

            if (packages.Count == 0) return null;

            var basePackage = packages
                .Where(p => Math.Abs(p.PriceFactor - 1d) < 0.00001d)
                .OrderBy(p => p.DurationHours)
                .ThenBy(p => p.PackageId)
                .FirstOrDefault();

            if (basePackage != null) return basePackage;

            return packages
                .OrderBy(p => p.PackageId)
                .FirstOrDefault();
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
