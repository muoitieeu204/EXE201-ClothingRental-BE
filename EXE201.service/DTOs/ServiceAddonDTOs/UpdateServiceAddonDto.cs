using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.ServiceAddonDTOs
{
    public class UpdateServiceAddonDto
    {
        public int? ServicePkgId { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
    }
}
