using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class ReviewImage
{
    public int ImgId { get; set; }

    public int ReviewId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Review Review { get; set; } = null!;
}
