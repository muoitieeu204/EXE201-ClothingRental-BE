using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs
{
    public class WishlistDTO
    {
        public int WishlistId { get; set; }

        public int UserId { get; set; }

        public int OutfitId { get; set; }

        public DateTime? AddedAt { get; set; }
    }
}
