using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Outfit> Outfits { get; set; } = new List<Outfit>();
}
