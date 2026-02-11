namespace EXE201.Service.DTOs.ServicePackageDTOs
{
    public class ServicePackageDetailDto
    {
 public int ServicePkgId { get; set; }
        public int StudioId { get; set; }
   public string? StudioName { get; set; }
public string? StudioAddress { get; set; }
     public string? StudioContactInfo { get; set; }
  public string Name { get; set; } = null!;
   public string? Description { get; set; }
  public decimal BasePrice { get; set; }
  public List<ServiceAddonInfo>? Addons { get; set; }
   public int TotalBookings { get; set; }
    }

    public class ServiceAddonInfo
    {
        public int AddonId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
