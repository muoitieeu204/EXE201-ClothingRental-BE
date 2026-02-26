using EXE201.Service.DTOs.DashboardDTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "SuperAdminOnly")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        /// <summary>
        /// Aggregate dashboard data for admin panel:
        /// summary cards, revenue-order trend, category distribution, recent activities, top products.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] int months = 6,
            [FromQuery] int topProducts = 5,
            [FromQuery] int activities = 10,
            [FromQuery] string categoryMetric = "rentals")
        {
            var query = new AdminDashboardQueryDto
            {
                Months = months,
                TopProducts = topProducts,
                Activities = activities,
                CategoryMetric = categoryMetric
            };

            var result = await _adminDashboardService.GetDashboardAsync(query);
            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _adminDashboardService.GetSummaryAsync();
            return Ok(result);
        }

        [HttpGet("revenue-orders-trend")]
        public async Task<IActionResult> GetRevenueOrdersTrend([FromQuery] int months = 6)
        {
            var result = await _adminDashboardService.GetRevenueOrdersTrendAsync(months);
            return Ok(result);
        }

        [HttpGet("category-distribution")]
        public async Task<IActionResult> GetCategoryDistribution([FromQuery(Name = "metric")] string categoryMetric = "rentals")
        {
            var result = await _adminDashboardService.GetCategoryDistributionAsync(categoryMetric);
            return Ok(result);
        }

        [HttpGet("recent-activities")]
        public async Task<IActionResult> GetRecentActivities([FromQuery(Name = "limit")] int activities = 10)
        {
            var result = await _adminDashboardService.GetRecentActivitiesAsync(activities);
            return Ok(result);
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery(Name = "limit")] int topProducts = 5, [FromQuery] string? period = null)
        {
            _ = period; // reserved for future periods (e.g., currentMonth, lastMonth, custom)
            var result = await _adminDashboardService.GetTopProductsAsync(topProducts);
            return Ok(result);
        }
    }
}
