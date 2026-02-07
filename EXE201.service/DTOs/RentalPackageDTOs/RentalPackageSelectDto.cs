using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.RentalPackageDTOs
{
    // Dùng cho dropdown / select package nhanh
    public class RentalPackageSelectDto
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DurationHours { get; set; }
    }
}
