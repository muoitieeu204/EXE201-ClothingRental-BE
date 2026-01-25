using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs
{
    public class OutfitImageDTO
    {
        [Required]
        public int OutfitId { get; set; }
        [Required]
        public string ImageUrl { get; set; } = null!;
        [Required]
        public string? ImageType { get; set; }

        public int? SortOrder { get; set; }
    }
}
