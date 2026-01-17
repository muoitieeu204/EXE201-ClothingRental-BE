using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class Outfit
{
    public int OutfitId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public string? Gender { get; set; }

    public string? Region { get; set; }

    public bool? IsLimited { get; set; }

    public string? Status { get; set; }

    public decimal BaseRentalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual OutfitAttribute? OutfitAttribute { get; set; }

    public virtual ICollection<OutfitImage> OutfitImages { get; set; } = new List<OutfitImage>();

    public virtual ICollection<OutfitSize> OutfitSizes { get; set; } = new List<OutfitSize>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
