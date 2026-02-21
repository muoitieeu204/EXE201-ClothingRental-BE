using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.LoyaltyTransactionDTOs
{
    public class LoyaltyTransactionDto
    {
        public string? FullName { get; set; }
        public int PointsAmount { get; set; }
        public string? TransactionType { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
