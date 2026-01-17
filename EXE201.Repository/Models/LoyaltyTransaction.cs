using System;
using System.Collections.Generic;

namespace EXE201.Repository.Models;

public partial class LoyaltyTransaction
{
    public int TransactionId { get; set; }

    public int UserId { get; set; }

    public int PointsAmount { get; set; }

    public string? TransactionType { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
