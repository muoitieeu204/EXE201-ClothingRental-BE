using EXE201.Service.DTOs.DashboardDTOs;

namespace EXE201.Service.Interface
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardResponseDto> GetDashboardAsync(AdminDashboardQueryDto query);
        Task<AdminDashboardSummaryDto> GetSummaryAsync();
        Task<List<AdminDashboardTrendPointDto>> GetRevenueOrdersTrendAsync(int months);
        Task<List<AdminDashboardCategoryDistributionItemDto>> GetCategoryDistributionAsync(string categoryMetric);
        Task<List<AdminDashboardRecentActivityDto>> GetRecentActivitiesAsync(int limit);
        Task<List<AdminDashboardTopProductDto>> GetTopProductsAsync(int limit);
    }
}
