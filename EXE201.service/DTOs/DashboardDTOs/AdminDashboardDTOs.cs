using System;
using System.Collections.Generic;

namespace EXE201.Service.DTOs.DashboardDTOs
{
    public class AdminDashboardQueryDto
    {
        public int Months { get; set; } = 6;
        public int TopProducts { get; set; } = 5;
        public int Activities { get; set; } = 10;
        public string CategoryMetric { get; set; } = "rentals"; // rentals | outfits
    }

    public class AdminDashboardResponseDto
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public AdminDashboardQueryDto AppliedQuery { get; set; } = new();
        public AdminDashboardSummaryDto Summary { get; set; } = new();
        public List<AdminDashboardTrendPointDto> RevenueOrdersTrend { get; set; } = new();
        public List<AdminDashboardCategoryDistributionItemDto> CategoryDistribution { get; set; } = new();
        public List<AdminDashboardRecentActivityDto> RecentActivities { get; set; } = new();
        public List<AdminDashboardTopProductDto> TopProducts { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }

    public class AdminDashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public decimal UsersChangePercent { get; set; }

        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public decimal ProductsChangePercent { get; set; }

        public int OrdersInCurrentPeriod { get; set; }
        public decimal OrdersChangePercent { get; set; }

        public decimal RevenueInCurrentPeriod { get; set; }
        public decimal RevenueChangePercent { get; set; }
        public string Currency { get; set; } = "VND";
    }

    public class AdminDashboardTrendPointDto
    {
        public string PeriodKey { get; set; } = string.Empty; // 2026-02
        public string Label { get; set; } = string.Empty;     // T2
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class AdminDashboardCategoryDistributionItemDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Value { get; set; }
        public decimal Percent { get; set; }
        public string Metric { get; set; } = "rentals";
    }

    public class AdminDashboardRecentActivityDto
    {
        public string Type { get; set; } = string.Empty; // order_created | payment_paid | user_registered | booking_completed
        public string Title { get; set; } = string.Empty;
        public string? SubTitle { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? ReferenceType { get; set; } // booking | payment | user
        public int? ReferenceId { get; set; }
    }

    public class AdminDashboardTopProductDto
    {
        public int Rank { get; set; }
        public int OutfitId { get; set; }
        public string OutfitName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public int RentalCount { get; set; }
        public decimal TrendPercent { get; set; }
    }
}
