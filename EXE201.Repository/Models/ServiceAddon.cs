using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class ServiceAddon
{
    public int AddonId { get; set; }

    public int ServicePkgId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<ServiceBookingAddon> ServiceBookingAddons { get; set; } = new List<ServiceBookingAddon>();

    public virtual ServicePackage ServicePkg { get; set; } = null!;
}
