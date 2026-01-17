using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class OutfitImage
{
    public int ImageId { get; set; }

    public int OutfitId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? ImageType { get; set; }

    public int? SortOrder { get; set; }

    public virtual Outfit Outfit { get; set; } = null!;
}
