using System;
using System.Collections.Generic;

namespace EXE201.Service.DTOs.AnalyticsDTOs
{
    public class AdminAnalyticsQueryDto
    {
        public string Range { get; set; } = "month"; // week | month | quarter | year | custom
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? GroupBy { get; set; } // day | month
        public int TopProducts { get; set; } = 5;
        public string CategoryMetric { get; set; } = "rentals"; // rentals | outfits
    }

    public class AdminAnalyticsAppliedQueryDto
    {
        public string Range { get; set; } = "month";
        public string GroupBy { get; set; } = "day";
        public int TopProducts { get; set; } = 5;
        public string CategoryMetric { get; set; } = "rentals";
        public DateTime RangeStartUtc { get; set; }
        public DateTime RangeEndUtc { get; set; } // exclusive
    }

    public class AdminAnalyticsResponseDto
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public AdminAnalyticsAppliedQueryDto AppliedQuery { get; set; } = new();
        public AdminAnalyticsOverviewDto Overview { get; set; } = new();
        public List<AdminAnalyticsTrendPointDto> Trend { get; set; } = new();
        public List<AdminAnalyticsCategoryDistributionItemDto> CategoryDistribution { get; set; } = new();
        public List<AdminAnalyticsTopProductDto> TopProducts { get; set; } = new();
        public List<AdminAnalyticsPeakHourDto> PeakHours { get; set; } = new();
        public List<AdminAnalyticsCustomerDemographicDto> CustomerDemographics { get; set; } = new();
        public AdminAnalyticsAvailabilityDto Availability { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }

    public class AdminAnalyticsOverviewDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueChangePercent { get; set; }

        public int TotalOrders { get; set; }
        public decimal OrdersChangePercent { get; set; }

        public int NewCustomers { get; set; }
        public decimal NewCustomersChangePercent { get; set; }

        public int? TotalViews { get; set; }
        public decimal? ViewsChangePercent { get; set; }

        public string Currency { get; set; } = "VND";
    }

    public class AdminAnalyticsTrendPointDto
    {
        public string PeriodKey { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public int NewCustomers { get; set; }
    }

    public class AdminAnalyticsCategoryDistributionItemDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Value { get; set; }
        public decimal Percent { get; set; }
        public string Metric { get; set; } = "rentals";
    }

    public class AdminAnalyticsTopProductDto
    {
        public int Rank { get; set; }
        public int OutfitId { get; set; }
        public string OutfitName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public int? Views { get; set; }
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
        public string Currency { get; set; } = "VND";
    }

    public class AdminAnalyticsPeakHourDto
    {
        public int Hour { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Orders { get; set; }
    }

    public class AdminAnalyticsCustomerDemographicDto
    {
        public string BucketKey { get; set; } = string.Empty;
        public string BucketLabel { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percent { get; set; }
    }

    public class AdminAnalyticsAvailabilityDto
    {
        public bool HasProductViews { get; set; }
        public bool HasCustomerAge { get; set; }
        public bool HasRevenueTrend { get; set; } = true;
        public bool HasCategoryDistribution { get; set; } = true;
        public bool HasTopProducts { get; set; } = true;
        public bool HasPeakHours { get; set; } = true;
    }
}
