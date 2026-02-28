using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.DashboardDTOs;
using EXE201.Service.Interface;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace EXE201.Service.Implementation
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _memoryCache;
        private static readonly TimeSpan DashboardCacheTtl = TimeSpan.FromSeconds(30);
        private const string DashboardSourceCacheKey = "admin-dashboard:source-data";

        public AdminDashboardService(IUnitOfWork unitOfWork, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _memoryCache = memoryCache;
        }

        public async Task<AdminDashboardResponseDto> GetDashboardAsync(AdminDashboardQueryDto query)
        {
            var normalizedQuery = NormalizeQuery(query);
            var cacheKey = BuildCacheKey(normalizedQuery);

            return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = DashboardCacheTtl;
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                entry.SetPriority(CacheItemPriority.High);
                return await BuildDashboardResponseAsync(normalizedQuery);
            }) ?? new AdminDashboardResponseDto { AppliedQuery = normalizedQuery };
        }

        public async Task<AdminDashboardSummaryDto> GetSummaryAsync()
        {
            var cacheKey = "admin-dashboard:summary";
            return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = DashboardCacheTtl;
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);

                var now = DateTime.UtcNow;
                var currentMonthStart = StartOfMonth(now);
                var data = await LoadSourceDataCachedAsync();
                return BuildSummary(data, currentMonthStart);
            }) ?? new AdminDashboardSummaryDto();
        }

        public async Task<List<AdminDashboardTrendPointDto>> GetRevenueOrdersTrendAsync(int months)
        {
            var normalized = NormalizeQuery(new AdminDashboardQueryDto { Months = months });
            var cacheKey = $"admin-dashboard:trend:m{normalized.Months}";

            return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = DashboardCacheTtl;
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);

                var now = DateTime.UtcNow;
                var currentMonthStart = StartOfMonth(now);
                var data = await LoadSourceDataCachedAsync();
                return BuildRevenueOrdersTrend(data, currentMonthStart, normalized.Months);
            }) ?? new List<AdminDashboardTrendPointDto>();
        }

        public async Task<List<AdminDashboardCategoryDistributionItemDto>> GetCategoryDistributionAsync(string categoryMetric)
        {
            var normalizedMetric = NormalizeCategoryMetric(categoryMetric);
            var cacheKey = $"admin-dashboard:category:{normalizedMetric}";

            return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = DashboardCacheTtl;
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);

                var data = await LoadSourceDataCachedAsync();
                return BuildCategoryDistribution(data, normalizedMetric);
            }) ?? new List<AdminDashboardCategoryDistributionItemDto>();
        }

        public async Task<List<AdminDashboardRecentActivityDto>> GetRecentActivitiesAsync(int limit)
        {
            var normalized = NormalizeQuery(new AdminDashboardQueryDto { Activities = limit });
            var cacheKey = $"admin-dashboard:activities:a{normalized.Activities}";

            return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = DashboardCacheTtl;
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);

                var data = await LoadSourceDataCachedAsync();
                return BuildRecentActivities(data, normalized.Activities);
            }) ?? new List<AdminDashboardRecentActivityDto>();
        }

        public async Task<List<AdminDashboardTopProductDto>> GetTopProductsAsync(int limit)
        {
            var normalized = NormalizeQuery(new AdminDashboardQueryDto { TopProducts = limit });
            var cacheKey = $"admin-dashboard:top-products:t{normalized.TopProducts}";

            return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = DashboardCacheTtl;
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);

                var now = DateTime.UtcNow;
                var currentMonthStart = StartOfMonth(now);
                var data = await LoadSourceDataCachedAsync();
                return BuildTopProducts(data, currentMonthStart, normalized.TopProducts);
            }) ?? new List<AdminDashboardTopProductDto>();
        }

        private async Task<AdminDashboardResponseDto> BuildDashboardResponseAsync(AdminDashboardQueryDto normalizedQuery)
        {
            var now = DateTime.UtcNow;
            var currentMonthStart = StartOfMonth(now);

            var data = await LoadSourceDataCachedAsync();

            var response = new AdminDashboardResponseDto
            {
                GeneratedAt = now,
                AppliedQuery = normalizedQuery,
                Summary = BuildSummary(data, currentMonthStart),
                RevenueOrdersTrend = BuildRevenueOrdersTrend(data, currentMonthStart, normalizedQuery.Months),
                CategoryDistribution = BuildCategoryDistribution(data, normalizedQuery.CategoryMetric),
                RecentActivities = BuildRecentActivities(data, normalizedQuery.Activities),
                TopProducts = BuildTopProducts(data, currentMonthStart, normalizedQuery.TopProducts)
            };

            response.Notes.Add("Recent activities currently include user registration, order creation, and paid payment events.");
            response.Notes.Add("Product view activity is not included because the current schema does not store view logs.");
            response.Notes.Add("Revenue is calculated from paid payment records (deposit/full) by PaymentTime.");

            return response;
        }

        private async Task<DashboardSourceData> LoadSourceDataCachedAsync()
        {
            return await _memoryCache.GetOrCreateAsync(DashboardSourceCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = DashboardCacheTtl;
                entry.SlidingExpiration = TimeSpan.FromSeconds(10);
                entry.SetPriority(CacheItemPriority.High);
                return await LoadSourceDataAsync();
            }) ?? new DashboardSourceData();
        }

        private async Task<DashboardSourceData> LoadSourceDataAsync()
        {
            var users = (await _unitOfWork.Users.GetAllAsync()).ToList();
            var bookings = (await _unitOfWork.Bookings.GetAllAsync()).ToList();
            var payments = (await _unitOfWork.Payments.GetAllAsync()).ToList();
            var bookingDetails = (await _unitOfWork.BookingDetails.GetAllAsync()).ToList();
            var outfits = (await _unitOfWork.Outfits.GetAllAsync()).ToList();
            var outfitSizes = (await _unitOfWork.OutfitSizes.GetAllAsync()).ToList();
            var categories = (await _unitOfWork.Categories.GetAllAsync()).ToList();

            return new DashboardSourceData
            {
                Users = users,
                Bookings = bookings,
                Payments = payments,
                BookingDetails = bookingDetails,
                Outfits = outfits,
                OutfitSizes = outfitSizes,
                Categories = categories,
                UserById = users.GroupBy(u => u.UserId).ToDictionary(g => g.Key, g => g.First()),
                BookingById = bookings.GroupBy(b => b.BookingId).ToDictionary(g => g.Key, g => g.First()),
                OutfitById = outfits.GroupBy(o => o.OutfitId).ToDictionary(g => g.Key, g => g.First()),
                OutfitSizeById = outfitSizes.GroupBy(s => s.SizeId).ToDictionary(g => g.Key, g => g.First()),
                CategoryById = categories.GroupBy(c => c.CategoryId).ToDictionary(g => g.Key, g => g.First())
            };
        }

        private static AdminDashboardQueryDto NormalizeQuery(AdminDashboardQueryDto? query)
        {
            query ??= new AdminDashboardQueryDto();
            return new AdminDashboardQueryDto
            {
                Months = Math.Clamp(query.Months, 1, 24),
                TopProducts = Math.Clamp(query.TopProducts, 1, 20),
                Activities = Math.Clamp(query.Activities, 1, 50),
                CategoryMetric = NormalizeCategoryMetric(query.CategoryMetric)
            };
        }

        private static string BuildCacheKey(AdminDashboardQueryDto query)
        {
            return $"admin-dashboard:m{query.Months}:t{query.TopProducts}:a{query.Activities}:c{query.CategoryMetric}";
        }

        private AdminDashboardSummaryDto BuildSummary(DashboardSourceData data, DateTime currentMonthStart)
        {
            var nextMonthStart = currentMonthStart.AddMonths(1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            var totalUsers = data.Users.Count(IsCountableUser);
            var currentMonthNewUsers = data.Users.Count(u => IsInRange(u.CreatedAt, currentMonthStart, nextMonthStart));
            var previousMonthNewUsers = data.Users.Count(u => IsInRange(u.CreatedAt, previousMonthStart, currentMonthStart));

            var totalProducts = data.Outfits.Count;
            var activeProducts = data.Outfits.Count(o => IsActiveOutfitStatus(o.Status));
            var currentMonthNewProducts = data.Outfits.Count(o => IsInRange(o.CreatedAt, currentMonthStart, nextMonthStart));
            var previousMonthNewProducts = data.Outfits.Count(o => IsInRange(o.CreatedAt, previousMonthStart, currentMonthStart));

            var currentMonthOrders = data.Bookings.Count(b =>
                !IsCancelledBookingStatus(b.Status) &&
                IsInRange(b.BookingDate, currentMonthStart, nextMonthStart));
            var previousMonthOrders = data.Bookings.Count(b =>
                !IsCancelledBookingStatus(b.Status) &&
                IsInRange(b.BookingDate, previousMonthStart, currentMonthStart));

            var currentMonthRevenue = data.Payments
                .Where(p => IsPaidPaymentStatus(p.Status) && IsInRange(p.PaymentTime, currentMonthStart, nextMonthStart))
                .Sum(p => p.Amount);

            var previousMonthRevenue = data.Payments
                .Where(p => IsPaidPaymentStatus(p.Status) && IsInRange(p.PaymentTime, previousMonthStart, currentMonthStart))
                .Sum(p => p.Amount);

            return new AdminDashboardSummaryDto
            {
                TotalUsers = totalUsers,
                UsersChangePercent = CalculatePercentChange(currentMonthNewUsers, previousMonthNewUsers),

                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                ProductsChangePercent = CalculatePercentChange(currentMonthNewProducts, previousMonthNewProducts),

                OrdersInCurrentPeriod = currentMonthOrders,
                OrdersChangePercent = CalculatePercentChange(currentMonthOrders, previousMonthOrders),

                RevenueInCurrentPeriod = RoundCurrency(currentMonthRevenue),
                RevenueChangePercent = CalculatePercentChange(currentMonthRevenue, previousMonthRevenue),
                Currency = "VND"
            };
        }

        private List<AdminDashboardTrendPointDto> BuildRevenueOrdersTrend(
            DashboardSourceData data,
            DateTime currentMonthStart,
            int months)
        {
            var firstMonthStart = currentMonthStart.AddMonths(-(months - 1));
            var rangeEnd = currentMonthStart.AddMonths(1);

            var orderGroups = data.Bookings
                .Where(b => !IsCancelledBookingStatus(b.Status) && IsInRange(b.BookingDate, firstMonthStart, rangeEnd))
                .GroupBy(b => ToMonthKey(b.BookingDate!.Value))
                .ToDictionary(g => g.Key, g => g.Count());

            var revenueGroups = data.Payments
                .Where(p => IsPaidPaymentStatus(p.Status) && IsInRange(p.PaymentTime, firstMonthStart, rangeEnd))
                .GroupBy(p => ToMonthKey(p.PaymentTime!.Value))
                .ToDictionary(g => g.Key, g => RoundCurrency(g.Sum(x => x.Amount)));

            var points = new List<AdminDashboardTrendPointDto>(months);
            for (var i = 0; i < months; i++)
            {
                var monthStart = firstMonthStart.AddMonths(i);
                var key = ToMonthKey(monthStart);

                points.Add(new AdminDashboardTrendPointDto
                {
                    PeriodKey = key,
                    Label = $"T{monthStart.Month}",
                    OrderCount = orderGroups.TryGetValue(key, out var orderCount) ? orderCount : 0,
                    Revenue = revenueGroups.TryGetValue(key, out var revenue) ? revenue : 0m
                });
            }

            return points;
        }

        private List<AdminDashboardCategoryDistributionItemDto> BuildCategoryDistribution(
            DashboardSourceData data,
            string categoryMetric)
        {
            Dictionary<int, int> categoryCounts;

            if (string.Equals(categoryMetric, "outfits", StringComparison.OrdinalIgnoreCase))
            {
                categoryCounts = data.Outfits
                    .GroupBy(o => o.CategoryId)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            else
            {
                categoryCounts = new Dictionary<int, int>();

                foreach (var detail in data.BookingDetails)
                {
                    if (!data.BookingById.TryGetValue(detail.BookingId, out var booking))
                    {
                        continue;
                    }

                    if (IsCancelledBookingStatus(booking.Status))
                    {
                        continue;
                    }

                    if (!data.OutfitSizeById.TryGetValue(detail.OutfitSizeId, out var outfitSize))
                    {
                        continue;
                    }

                    if (!data.OutfitById.TryGetValue(outfitSize.OutfitId, out var outfit))
                    {
                        continue;
                    }

                    if (!categoryCounts.TryAdd(outfit.CategoryId, 1))
                    {
                        categoryCounts[outfit.CategoryId]++;
                    }
                }
            }

            var total = categoryCounts.Values.Sum();
            if (total <= 0)
            {
                return new List<AdminDashboardCategoryDistributionItemDto>();
            }

            return categoryCounts
                .Where(kv => kv.Value > 0)
                .Select(kv =>
                {
                    data.CategoryById.TryGetValue(kv.Key, out var category);
                    return new AdminDashboardCategoryDistributionItemDto
                    {
                        CategoryId = kv.Key,
                        CategoryName = category?.CategoryName ?? $"Danh mục #{kv.Key}",
                        Value = kv.Value,
                        Percent = Math.Round((decimal)kv.Value * 100m / total, 2, MidpointRounding.AwayFromZero),
                        Metric = categoryMetric
                    };
                })
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.CategoryName)
                .ToList();
        }

        private List<AdminDashboardRecentActivityDto> BuildRecentActivities(DashboardSourceData data, int limit)
        {
            var activities = new List<AdminDashboardRecentActivityDto>();

            activities.AddRange(data.Users
                .Where(u => u.CreatedAt.HasValue)
                .Select(u => new AdminDashboardRecentActivityDto
                {
                    Type = "user_registered",
                    Title = $"{DisplayUserName(u)} đăng ký tài khoản",
                    SubTitle = string.IsNullOrWhiteSpace(u.Email) ? null : u.Email,
                    OccurredAt = u.CreatedAt!.Value,
                    ReferenceType = "user",
                    ReferenceId = u.UserId
                }));

            activities.AddRange(data.Bookings
                .Where(b => b.BookingDate.HasValue)
                .Select(b =>
                {
                    data.UserById.TryGetValue(b.UserId, out var user);
                    return new AdminDashboardRecentActivityDto
                    {
                        Type = "order_created",
                        Title = $"{DisplayUserName(user)} đặt đơn thuê {ToOrderCode(b.BookingId)}",
                        SubTitle = string.IsNullOrWhiteSpace(b.PaymentStatus) ? null : $"Trạng thái thanh toán: {b.PaymentStatus}",
                        OccurredAt = b.BookingDate!.Value,
                        ReferenceType = "booking",
                        ReferenceId = b.BookingId
                    };
                }));

            activities.AddRange(data.Payments
                .Where(p => IsPaidPaymentStatus(p.Status) && p.PaymentTime.HasValue)
                .Select(p =>
                {
                    data.BookingById.TryGetValue(p.BookingId, out var booking);
                    data.UserById.TryGetValue(booking?.UserId ?? 0, out var user);
                    return new AdminDashboardRecentActivityDto
                    {
                        Type = "payment_paid",
                        Title = $"{DisplayUserName(user)} thanh toán {ToOrderCode(p.BookingId)}",
                        SubTitle = $"{FormatCurrencyVnd(p.Amount)} ({p.PaymentMethod})",
                        OccurredAt = p.PaymentTime!.Value,
                        ReferenceType = "payment",
                        ReferenceId = p.PaymentId
                    };
                }));

            return activities
                .OrderByDescending(a => a.OccurredAt)
                .Take(limit)
                .ToList();
        }

        private List<AdminDashboardTopProductDto> BuildTopProducts(
            DashboardSourceData data,
            DateTime currentMonthStart,
            int topN)
        {
            var currentMonthEnd = currentMonthStart.AddMonths(1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            var currentCounts = new Dictionary<int, int>();
            var previousCounts = new Dictionary<int, int>();

            foreach (var detail in data.BookingDetails)
            {
                if (!data.BookingById.TryGetValue(detail.BookingId, out var booking))
                {
                    continue;
                }

                if (IsCancelledBookingStatus(booking.Status))
                {
                    continue;
                }

                if (!data.OutfitSizeById.TryGetValue(detail.OutfitSizeId, out var outfitSize))
                {
                    continue;
                }

                var bookingDate = booking.BookingDate ?? detail.StartTime;
                if (!bookingDate.HasValue)
                {
                    continue;
                }

                if (bookingDate.Value >= currentMonthStart && bookingDate.Value < currentMonthEnd)
                {
                    Increment(currentCounts, outfitSize.OutfitId);
                }
                else if (bookingDate.Value >= previousMonthStart && bookingDate.Value < currentMonthStart)
                {
                    Increment(previousCounts, outfitSize.OutfitId);
                }
            }

            var results = currentCounts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => data.OutfitById.TryGetValue(kv.Key, out var outfit) ? outfit.Name : kv.Key.ToString())
                .Take(topN)
                .Select((kv, index) =>
                {
                    data.OutfitById.TryGetValue(kv.Key, out var outfit);
                    var previousCount = previousCounts.TryGetValue(kv.Key, out var c) ? c : 0;

                    string? categoryName = null;
                    if (outfit != null && data.CategoryById.TryGetValue(outfit.CategoryId, out var category))
                    {
                        categoryName = category.CategoryName;
                    }

                    return new AdminDashboardTopProductDto
                    {
                        Rank = index + 1,
                        OutfitId = kv.Key,
                        OutfitName = outfit?.Name ?? $"Sản phẩm #{kv.Key}",
                        CategoryName = categoryName,
                        RentalCount = kv.Value,
                        TrendPercent = CalculatePercentChange(kv.Value, previousCount)
                    };
                })
                .ToList();

            return results;
        }

        private static string NormalizeCategoryMetric(string? value)
        {
            return string.Equals(value, "outfits", StringComparison.OrdinalIgnoreCase)
                ? "outfits"
                : "rentals";
        }

        private static DateTime StartOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        private static bool IsInRange(DateTime? value, DateTime startInclusive, DateTime endExclusive)
        {
            return value.HasValue && value.Value >= startInclusive && value.Value < endExclusive;
        }

        private static string ToMonthKey(DateTime date)
        {
            return $"{date.Year:D4}-{date.Month:D2}";
        }

        private static bool IsCountableUser(User user)
        {
            return user.IsActive != false;
        }

        private static bool IsCancelledBookingStatus(string? status)
        {
            return string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPaidPaymentStatus(string? status)
        {
            return string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsActiveOutfitStatus(string? status)
        {
            return string.Equals(status, "Available", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);
        }

        private static decimal CalculatePercentChange(int currentValue, int previousValue)
        {
            return CalculatePercentChange((decimal)currentValue, (decimal)previousValue);
        }

        private static decimal CalculatePercentChange(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0)
            {
                if (currentValue == 0)
                {
                    return 0m;
                }

                return 100m;
            }

            var percent = ((currentValue - previousValue) / previousValue) * 100m;
            return Math.Round(percent, 2, MidpointRounding.AwayFromZero);
        }

        private static void Increment(Dictionary<int, int> dictionary, int key)
        {
            if (!dictionary.TryAdd(key, 1))
            {
                dictionary[key]++;
            }
        }

        private static decimal RoundCurrency(decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        private static string FormatCurrencyVnd(decimal amount)
        {
            return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0}đ", amount);
        }

        private static string DisplayUserName(User? user)
        {
            if (user == null)
            {
                return "Khách hàng";
            }

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                return user.FullName;
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                return user.Email;
            }

            return $"User#{user.UserId}";
        }

        private static string ToOrderCode(int bookingId)
        {
            return $"ORD{bookingId:D3}";
        }

        private sealed class DashboardSourceData
        {
            public List<User> Users { get; set; } = new();
            public List<Booking> Bookings { get; set; } = new();
            public List<Payment> Payments { get; set; } = new();
            public List<BookingDetail> BookingDetails { get; set; } = new();
            public List<Outfit> Outfits { get; set; } = new();
            public List<OutfitSize> OutfitSizes { get; set; } = new();
            public List<Category> Categories { get; set; } = new();

            public Dictionary<int, User> UserById { get; set; } = new();
            public Dictionary<int, Booking> BookingById { get; set; } = new();
            public Dictionary<int, Outfit> OutfitById { get; set; } = new();
            public Dictionary<int, OutfitSize> OutfitSizeById { get; set; } = new();
            public Dictionary<int, Category> CategoryById { get; set; } = new();
        }
    }
}
