namespace EXE201.Service.DTOs.ServicePackageDTOs
{
    public class ServicePackageResponseDto
    {
        public int ServicePkgId { get; set; }
        public int StudioId { get; set; }
  public string? StudioName { get; set; }
   public string Name { get; set; } = null!;
   public string? Description { get; set; }
 public decimal BasePrice { get; set; }
 public int TotalAddons { get; set; }
  public int TotalBookings { get; set; }
    }
}
