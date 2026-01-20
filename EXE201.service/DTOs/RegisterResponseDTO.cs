using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs
{
  public class RegisterResponseDTO
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int RoleId { get; set; }
        public string Message { get; set; } = "User registered successfully";
    }
}
