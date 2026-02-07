namespace EXE201.Service.DTOs.ServiceBookingDTOs
{
    public class ServiceBookingDetailDto
    {
 public int SvcBookingId { get; set; }
public int UserId { get; set; }
        public string? UserFullName { get; set; }
  public string? UserEmail { get; set; }
        public string? UserPhoneNumber { get; set; }
        public int? BookingId { get; set; }
   public int ServicePkgId { get; set; }
      public string? ServicePackageName { get; set; }
   public string? ServicePackageDescription { get; set; }
        public decimal? ServicePackageBasePrice { get; set; }
      public string? StudioName { get; set; }
        public string? StudioAddress { get; set; }
        public string? StudioContactInfo { get; set; }
 public DateTime? ServiceTime { get; set; }
        public decimal? TotalPrice { get; set; }
    public string? Status { get; set; }
   public List<ServiceBookingAddonInfo>? Addons { get; set; }
    }

 public class ServiceBookingAddonInfo
    {
        public int Id { get; set; }
 public int AddonId { get; set; }
        public string? AddonName { get; set; }
   public decimal? PriceAtBooking { get; set; }
    }
}
