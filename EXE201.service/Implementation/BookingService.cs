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

        public async Task<IEnumerable<BookingAdminV2Dto>> GetAllV2Async(
                bool includeDetails = true,
                bool includeServices = false
            )
        {
            var bookings = await _uow.Bookings.GetAllAsync();
            var result = _mapper.Map<List<BookingAdminV2Dto>>(bookings);

            // Prefetch users để lấy FullName + Email (tránh N+1)
            var userIds = bookings.Select(b => b.UserId).Distinct().ToList();

            var users = userIds.Count == 0
                ? new List<User>()
                : (await _uow.Users.FindAsync(u => userIds.Contains(u.UserId))).ToList();

            var userById = users.ToDictionary(u => u.UserId, u => u);

            foreach (var dto in result)
            {
                // 1) user info
                if (userById.TryGetValue(dto.UserId, out var user))
                {
                    dto.UserFullName = user.FullName ?? string.Empty;
                    dto.UserEmail = user.Email ?? string.Empty;
                }
                else
                {
                    dto.UserFullName = string.Empty;
                    dto.UserEmail = string.Empty;
                }

                // 2) details (optional)
                if (includeDetails)
                {
                    var detailsEntities = await _uow.BookingDetails.FindAsync(d => d.BookingId == dto.BookingId);
                    dto.Details = _mapper.Map<List<BookingDetailAdminV2Dto>>(detailsEntities);

                    // Nếu cốt muốn outfitName/rentalPackageName/image... chuẩn thì mở dòng này
                    // await EnrichBookingDetailDtosAsync(dto.Details, detailsEntities);
                }
                else
                {
                    dto.Details = new List<BookingDetailAdminV2Dto>();
                }

                if (includeDetails)
                {
                    var detailsEntities = await _uow.BookingDetails.FindAsync(d => d.BookingId == dto.BookingId);
                    dto.Details = _mapper.Map<List<BookingDetailAdminV2Dto>>(detailsEntities);

                    // ✅ gọi overload V2 vừa thêm
                    await EnrichBookingDetailDtosAsync(dto.Details, detailsEntities);
                }
                else
                {
                    dto.Details = new List<BookingDetailAdminV2Dto>();
                }
            }

            return result;
        }

        public async Task<BookingAdminV2Dto?> GetByIdV2Async(
                int bookingId,
                bool includeDetails = true,
                bool includeServices = true
            )
        {
            var booking = await _uow.Bookings.GetByIdAsync(bookingId);
            if (booking == null) return null;

            var dto = _mapper.Map<BookingAdminV2Dto>(booking);

            // user info
            var user = await _uow.Users.GetByIdAsync(dto.UserId);
            dto.UserFullName = user?.FullName ?? string.Empty;
            dto.UserEmail = user?.Email ?? string.Empty;

            // details
            if (includeDetails)
            {
                var detailsEntities = await _uow.BookingDetails.FindAsync(d => d.BookingId == bookingId);
                dto.Details = _mapper.Map<List<BookingDetailAdminV2Dto>>(detailsEntities);

                // ✅ enrich V2 details (outfitName/type/size/image + packageName)
                await EnrichBookingDetailDtosAsync(dto.Details, detailsEntities);
            }
            else
            {
                dto.Details = new List<BookingDetailAdminV2Dto>();
            }

            // services + addons (full như hình)
            if (includeServices)
            {
                var servicesEntities = await _uow.ServiceBookings.FindAsync(s => s.BookingId == bookingId);
                dto.Services = _mapper.Map<List<ServiceBookingDto>>(servicesEntities);

                // ✅ enrich như GetMy (servicePackageName + studioName)
                await EnrichServiceBookingDtosAsync(dto.Services, servicesEntities);

                // ✅ addons
                foreach (var s in dto.Services)
                {
                    var addons = await _uow.ServiceBookingAddons.FindAsync(a => a.SvcBookingId == s.SvcBookingId);
                    s.Addons = _mapper.Map<List<ServiceBookingAddonDto>>(addons);
                }
            }
            else
            {
                dto.Services = new List<ServiceBookingDto>();
            }

            // ✅ OPTIONAL nhưng nên có: tính totals cho đẹp (đỡ 0đ)
            // Nếu cốt muốn "không tính gì", comment dòng này.
            await PopulateBookingComputedTotalsV2Async(dto);

            return dto;
        }

        // helper totals V2 (y hệt hàm cũ, chỉ đổi type + Services là ServiceBookingDto)
        private async Task PopulateBookingComputedTotalsV2Async(BookingAdminV2Dto? bookingDto)
        {
            if (bookingDto == null || bookingDto.BookingId <= 0) return;

            decimal totalService;
            if (bookingDto.Services != null && bookingDto.Services.Count > 0)
                totalService = bookingDto.Services.Sum(s => s.TotalPrice ?? 0m);
            else
            {
                var serviceBookings = await _uow.ServiceBookings.FindAsync(s => s.BookingId == bookingDto.BookingId);
                totalService = serviceBookings.Sum(s => s.TotalPrice ?? 0m);
            }

            var totalRental = bookingDto.TotalRentalAmount ?? 0m;
            var totalSurcharge = bookingDto.TotalSurcharge ?? 0m;

            bookingDto.TotalServiceAmount = RoundCurrency(totalService);
            bookingDto.TotalOrderAmount = RoundCurrency(totalRental + totalSurcharge + (bookingDto.TotalServiceAmount ?? 0m));
        }


        public async Task<IEnumerable<BookingDto>> GetMyBookingsAsync(int userId, bool includeDetails = false, bool includeServices = false)
        {
            var bookings = await _uow.Bookings.GetBookingsByUserIdAsync(userId);
            var result = _mapper.Map<List<BookingDto>>(bookings);

            foreach (var b in result)
            {
                if (includeDetails)
                {
                    var details = await _uow.BookingDetails.FindAsync(d => d.BookingId == b.BookingId);
                    b.Details = _mapper.Map<List<BookingDetailDto>>(details);
                    await EnrichBookingDetailDtosAsync(b.Details, details);
                }

                if (includeServices)
                {
                    var services = await _uow.ServiceBookings.FindAsync(s => s.BookingId == b.BookingId);
                    b.Services = _mapper.Map<List<ServiceBookingDto>>(services);
                    await EnrichServiceBookingDtosAsync(b.Services, services);

                    foreach (var s in b.Services)
                    {
                        var addons = await _uow.ServiceBookingAddons.FindAsync(a => a.SvcBookingId == s.SvcBookingId);
                        s.Addons = _mapper.Map<List<ServiceBookingAddonDto>>(addons);
                    }
                }

                await PopulateBookingComputedTotalsAsync(b);
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
                await EnrichBookingDetailDtosAsync(dto.Details, details);
            }

            if (includeServices)
            {
                var services = await _uow.ServiceBookings.FindAsync(s => s.BookingId == bookingId);
                dto.Services = _mapper.Map<List<ServiceBookingDto>>(services);
                await EnrichServiceBookingDtosAsync(dto.Services, services);

                foreach (var s in dto.Services)
                {
                    var addons = await _uow.ServiceBookingAddons.FindAsync(a => a.SvcBookingId == s.SvcBookingId);
                    s.Addons = _mapper.Map<List<ServiceBookingAddonDto>>(addons);
                }
            }

            await PopulateBookingComputedTotalsAsync(dto);

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

            foreach (var item in dto.Items)
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

        private async Task EnrichBookingDetailDtosAsync(
            List<BookingDetailDto>? detailDtos,
            IEnumerable<BookingDetail>? detailEntities)
        {
            if (detailDtos == null || detailDtos.Count == 0 || detailEntities == null)
                return;

            var entityList = detailEntities.ToList();
            if (entityList.Count == 0)
                return;

            var entityByDetailId = entityList.ToDictionary(d => d.DetailId, d => d);

            var outfitSizeIds = entityList
                .Select(d => d.OutfitSizeId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var outfitSizes = outfitSizeIds.Count == 0
                ? new List<OutfitSize>()
                : (await _uow.OutfitSizes.FindAsync(s => outfitSizeIds.Contains(s.SizeId))).ToList();

            var outfitSizeById = outfitSizes.ToDictionary(s => s.SizeId, s => s);

            var outfitIds = outfitSizes
                .Select(s => s.OutfitId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var outfits = outfitIds.Count == 0
                ? new List<Outfit>()
                : (await _uow.Outfits.FindAsync(o => outfitIds.Contains(o.OutfitId))).ToList();

            var outfitById = outfits.ToDictionary(o => o.OutfitId, o => o);

            var outfitImages = outfitIds.Count == 0
                ? new List<OutfitImage>()
                : (await _uow.OutfitImages.FindAsync(img => outfitIds.Contains(img.OutfitId))).ToList();

            var primaryImageByOutfitId = outfitImages
                .GroupBy(img => img.OutfitId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(img => img.SortOrder ?? int.MaxValue)
                          .ThenBy(img => img.ImageId)
                          .Select(img => img.ImageUrl)
                          .FirstOrDefault());

            var rentalPackageIds = entityList
                .Where(d => d.RentalPackageId.HasValue && d.RentalPackageId.Value > 0)
                .Select(d => d.RentalPackageId!.Value)
                .Distinct()
                .ToList();

            var rentalPackages = rentalPackageIds.Count == 0
                ? new List<RentalPackage>()
                : (await _uow.RentalPackages.FindAsync(p => rentalPackageIds.Contains(p.PackageId))).ToList();

            var rentalPackageById = rentalPackages.ToDictionary(p => p.PackageId, p => p);

            foreach (var dto in detailDtos)
            {
                if (!entityByDetailId.TryGetValue(dto.DetailId, out var entity))
                    continue;

                if (entity.RentalPackageId.HasValue && entity.RentalPackageId.Value > 0)
                {
                    dto.RentalPackageId = entity.RentalPackageId.Value;
                    if (rentalPackageById.TryGetValue(entity.RentalPackageId.Value, out var rentalPackage))
                    {
                        dto.RentalPackageName = rentalPackage.Name;
                    }
                }

                if (outfitSizeById.TryGetValue(entity.OutfitSizeId, out var outfitSize))
                {
                    dto.OutfitId = outfitSize.OutfitId;
                    dto.OutfitSizeLabel = string.IsNullOrWhiteSpace(outfitSize.SizeLabel)
                        ? dto.OutfitSizeLabel
                        : outfitSize.SizeLabel;

                    if (outfitById.TryGetValue(outfitSize.OutfitId, out var outfit))
                    {
                        dto.OutfitName = string.IsNullOrWhiteSpace(outfit.Name)
                            ? dto.OutfitName
                            : outfit.Name;
                        dto.OutfitType = string.IsNullOrWhiteSpace(outfit.Type)
                            ? dto.OutfitType
                            : outfit.Type;
                    }

                    if (primaryImageByOutfitId.TryGetValue(outfitSize.OutfitId, out var imageUrl)
                        && !string.IsNullOrWhiteSpace(imageUrl))
                    {
                        dto.OutfitImageUrl = imageUrl;
                    }
                }

                var rentalDays = CalculateRentalDays(entity.StartTime, entity.EndTime);
                if (rentalDays.HasValue && rentalDays.Value > 0)
                {
                    dto.RentalDays = rentalDays.Value;
                }
            }
        }

        private async Task EnrichServiceBookingDtosAsync(
            List<ServiceBookingDto>? serviceDtos,
            IEnumerable<ServiceBooking>? serviceEntities)
        {
            if (serviceDtos == null || serviceDtos.Count == 0 || serviceEntities == null)
                return;

            var entityList = serviceEntities.ToList();
            if (entityList.Count == 0)
                return;

            var entityBySvcBookingId = entityList.ToDictionary(s => s.SvcBookingId, s => s);

            var servicePkgIds = entityList
                .Select(s => s.ServicePkgId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var servicePackages = servicePkgIds.Count == 0
                ? new List<ServicePackage>()
                : (await _uow.ServicePackages.FindAsync(p => servicePkgIds.Contains(p.ServicePkgId))).ToList();

            var servicePackageById = servicePackages.ToDictionary(p => p.ServicePkgId, p => p);

            var studioIds = servicePackages
                .Select(p => p.StudioId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var studios = studioIds.Count == 0
                ? new List<Studio>()
                : (await _uow.Studios.FindAsync(s => studioIds.Contains(s.StudioId))).ToList();

            var studioById = studios.ToDictionary(s => s.StudioId, s => s);

            foreach (var dto in serviceDtos)
            {
                if (!entityBySvcBookingId.TryGetValue(dto.SvcBookingId, out var entity))
                    continue;

                if (entity.ServicePkgId <= 0)
                    continue;

                if (servicePackageById.TryGetValue(entity.ServicePkgId, out var servicePackage))
                {
                    dto.ServicePackageName = string.IsNullOrWhiteSpace(servicePackage.Name)
                        ? dto.ServicePackageName
                        : servicePackage.Name;

                    if (studioById.TryGetValue(servicePackage.StudioId, out var studio))
                    {
                        dto.StudioName = string.IsNullOrWhiteSpace(studio.Name)
                            ? dto.StudioName
                            : studio.Name;
                    }
                }
            }
        }

        private async Task EnrichBookingServiceAdminV2DtosAsync(
    List<BookingServiceAdminV2Dto>? serviceDtos,
    IEnumerable<ServiceBooking>? serviceEntities
)
        {
            if (serviceDtos == null || serviceDtos.Count == 0 || serviceEntities == null)
                return;

            var entityList = serviceEntities.ToList();
            if (entityList.Count == 0) return;

            var pkgIds = entityList
                .Select(s => s.ServicePkgId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var pkgs = pkgIds.Count == 0
                ? new List<ServicePackage>()
                : (await _uow.ServicePackages.FindAsync(p => pkgIds.Contains(p.ServicePkgId))).ToList();

            var pkgById = pkgs.ToDictionary(p => p.ServicePkgId, p => p);

            foreach (var dto in serviceDtos)
            {
                if (!dto.ServiceId.HasValue) continue;

                if (pkgById.TryGetValue(dto.ServiceId.Value, out var pkg))
                {
                    dto.ServiceName = pkg.Name;
                    dto.Price = pkg.BasePrice;     // ✅ có BasePrice rồi
                    dto.Quantity ??= 1;
                }
            }
        }

        private async Task EnrichBookingDetailDtosAsync(
    List<BookingDetailAdminV2Dto>? detailDtos,
    IEnumerable<BookingDetail>? detailEntities)
        {
            if (detailDtos == null || detailDtos.Count == 0 || detailEntities == null)
                return;

            var entityList = detailEntities.ToList();
            if (entityList.Count == 0)
                return;

            var entityByDetailId = entityList.ToDictionary(d => d.DetailId, d => d);

            var outfitSizeIds = entityList
                .Select(d => d.OutfitSizeId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var outfitSizes = outfitSizeIds.Count == 0
                ? new List<OutfitSize>()
                : (await _uow.OutfitSizes.FindAsync(s => outfitSizeIds.Contains(s.SizeId))).ToList();

            var outfitSizeById = outfitSizes.ToDictionary(s => s.SizeId, s => s);

            var outfitIds = outfitSizes
                .Select(s => s.OutfitId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var outfits = outfitIds.Count == 0
                ? new List<Outfit>()
                : (await _uow.Outfits.FindAsync(o => outfitIds.Contains(o.OutfitId))).ToList();

            var outfitById = outfits.ToDictionary(o => o.OutfitId, o => o);

            var outfitImages = outfitIds.Count == 0
                ? new List<OutfitImage>()
                : (await _uow.OutfitImages.FindAsync(img => outfitIds.Contains(img.OutfitId))).ToList();

            var primaryImageByOutfitId = outfitImages
                .GroupBy(img => img.OutfitId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(img => img.SortOrder ?? int.MaxValue)
                          .ThenBy(img => img.ImageId)
                          .Select(img => img.ImageUrl)
                          .FirstOrDefault());

            var rentalPackageIds = entityList
                .Where(d => d.RentalPackageId.HasValue && d.RentalPackageId.Value > 0)
                .Select(d => d.RentalPackageId!.Value)
                .Distinct()
                .ToList();

            var rentalPackages = rentalPackageIds.Count == 0
                ? new List<RentalPackage>()
                : (await _uow.RentalPackages.FindAsync(p => rentalPackageIds.Contains(p.PackageId))).ToList();

            var rentalPackageById = rentalPackages.ToDictionary(p => p.PackageId, p => p);

            foreach (var dto in detailDtos)
            {
                if (!entityByDetailId.TryGetValue(dto.DetailId, out var entity))
                    continue;

                // rental package
                if (entity.RentalPackageId.HasValue && entity.RentalPackageId.Value > 0)
                {
                    dto.RentalPackageId = entity.RentalPackageId.Value;
                    if (rentalPackageById.TryGetValue(entity.RentalPackageId.Value, out var rentalPackage))
                    {
                        dto.RentalPackageName = rentalPackage.Name;
                    }
                }

                // outfit
                if (outfitSizeById.TryGetValue(entity.OutfitSizeId, out var outfitSize))
                {
                    dto.OutfitId = outfitSize.OutfitId;
                    dto.OutfitSizeLabel = string.IsNullOrWhiteSpace(outfitSize.SizeLabel)
                        ? dto.OutfitSizeLabel
                        : outfitSize.SizeLabel;

                    if (outfitById.TryGetValue(outfitSize.OutfitId, out var outfit))
                    {
                        dto.OutfitName = string.IsNullOrWhiteSpace(outfit.Name) ? dto.OutfitName : outfit.Name;
                        dto.OutfitType = string.IsNullOrWhiteSpace(outfit.Type) ? dto.OutfitType : outfit.Type;
                    }

                    if (primaryImageByOutfitId.TryGetValue(outfitSize.OutfitId, out var imageUrl)
                        && !string.IsNullOrWhiteSpace(imageUrl))
                    {
                        dto.OutfitImageUrl = imageUrl;
                    }
                }

                // rental days
                var rentalDays = CalculateRentalDays(entity.StartTime, entity.EndTime);
                if (rentalDays.HasValue && rentalDays.Value > 0)
                {
                    dto.RentalDays = rentalDays.Value;
                }
            }
        }

        private static int? CalculateRentalDays(DateTime? startTime, DateTime? endTime)
        {
            if (!startTime.HasValue || !endTime.HasValue)
                return null;

            var diff = endTime.Value - startTime.Value;
            if (diff <= TimeSpan.Zero)
                return null;

            var days = (int)Math.Ceiling(diff.TotalDays);
            return days > 0 ? days : null;
        }

        private async Task PopulateBookingComputedTotalsAsync(BookingDto? bookingDto)
        {
            if (bookingDto == null || bookingDto.BookingId <= 0)
                return;

            decimal totalService;
            if (bookingDto.Services != null && bookingDto.Services.Count > 0)
            {
                totalService = bookingDto.Services.Sum(s => s.TotalPrice ?? 0m);
            }
            else
            {
                var serviceBookings = await _uow.ServiceBookings.FindAsync(s => s.BookingId == bookingDto.BookingId);
                totalService = serviceBookings.Sum(s => s.TotalPrice ?? 0m);
            }

            var totalRental = bookingDto.TotalRentalAmount ?? 0m;
            var totalSurcharge = bookingDto.TotalSurcharge ?? 0m;

            bookingDto.TotalServiceAmount = RoundCurrency(totalService);
            bookingDto.TotalOrderAmount = RoundCurrency(totalRental + totalSurcharge + (bookingDto.TotalServiceAmount ?? 0m));
        }

        private static decimal RoundCurrency(decimal amount)
        {
            return Math.Round(amount, 0, MidpointRounding.AwayFromZero);
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

            // Chỉ cho hủy khi booking còn Pending
            if (!IsPendingStatus(booking.Status))
                return false;

            // Không cho hủy nếu booking đã “được xem là đã thanh toán” theo snapshot trong Booking
            if (IsPaidPaymentStatus(booking.PaymentStatus))
                return false;

            // Chặn thêm theo bảng Payment để tránh lệch nếu PaymentStatus chưa sync kịp
            var payments = await _uow.Payments.GetPaymentsByBookingIdAsync(bookingId);
            if (payments != null && payments.Any(p => IsSuccessfulPaymentRecord(p.Status)))
                return false;

            // Update status booking
            booking.Status = "Cancelled";

            // Cancel luôn details
            var details = await _uow.BookingDetails.FindAsync(d => d.BookingId == bookingId);
            if (details != null)
            {
                foreach (var d in details)
                {
                    d.Status = "Cancelled";
                    await _uow.BookingDetails.UpdateAsync(d);
                }
            }

            // Cancel luôn service bookings
            var serviceBookings = await _uow.ServiceBookings.GetServiceBookingsByBookingIdAsync(bookingId);
            if (serviceBookings != null)
            {
                foreach (var sb in serviceBookings)
                {
                    sb.Status = "Cancelled";
                    await _uow.ServiceBookings.UpdateAsync(sb);
                }
            }

            await _uow.Bookings.UpdateAsync(booking);
            await _uow.SaveChangesAsync();
            return true;
        }

        // =======================
        // Helpers
        // =======================

        private static bool IsPendingStatus(string? status)
            => string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase);

        private static bool IsPaidPaymentStatus(string? paymentStatus)
            => string.Equals(paymentStatus, "Paid", StringComparison.OrdinalIgnoreCase)
            || string.Equals(paymentStatus, "Success", StringComparison.OrdinalIgnoreCase)
            || string.Equals(paymentStatus, "Completed", StringComparison.OrdinalIgnoreCase);

        private static bool IsSuccessfulPaymentRecord(string? paymentStatus)
            => string.Equals(paymentStatus, "Paid", StringComparison.OrdinalIgnoreCase)
            || string.Equals(paymentStatus, "Success", StringComparison.OrdinalIgnoreCase)
            || string.Equals(paymentStatus, "Completed", StringComparison.OrdinalIgnoreCase);

    }

}
