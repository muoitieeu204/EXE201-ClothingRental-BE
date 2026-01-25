using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs
{
    public class OutfitSizeDTO
    {
        [Required]
        public int OutfitId { get; set; }
        [Required]
        public string SizeLabel { get; set; } = null!;
        [Required]
        public int? StockQuantity { get; set; }

        public double? ChestMaxCm { get; set; }

        public double? WaistMaxCm { get; set; }

        public double? HipMaxCm { get; set; }
        [Required]
        public string? Status { get; set; }
    }
}
