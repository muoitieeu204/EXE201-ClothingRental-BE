using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs
{
    public class CreateUserDTO
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password{ get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string? FullName { get; set; }

        [StringLength(100)]
        public string? PhoneNumber { get; set; }

    }
}
