namespace EXE201.Service.DTOs.ServiceBookingDTOs
{
    public class ServiceBookingResponseDto
    {
        public int SvcBookingId { get; set; }
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
        public int? BookingId { get; set; }
        public int ServicePkgId { get; set; }
        public string? ServicePackageName { get; set; }
        public string? StudioName { get; set; }
        public DateTime? ServiceTime { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? Status { get; set; }
        public int TotalAddons { get; set; }
    }
}
