using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class Studio
{
    public int StudioId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? ContactInfo { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<ServicePackage> ServicePackages { get; set; } = new List<ServicePackage>();
}
