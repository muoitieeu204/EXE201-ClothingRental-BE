using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class Wishlist
{
    public int WishlistId { get; set; }

    public int UserId { get; set; }

    public int OutfitId { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual Outfit Outfit { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
