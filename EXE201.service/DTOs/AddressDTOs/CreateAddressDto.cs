using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.AddressDTOs
{
    public class CreateAddressDto
    {
        [Required]
        [MaxLength(255)]
        public string? AddressText { get; set; }
    }
}
