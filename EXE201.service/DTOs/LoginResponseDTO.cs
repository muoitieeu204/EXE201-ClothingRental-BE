using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs
{
    public class LoginResponseDTO
    {
    public string Token { get; set; } = string.Empty;
        public UserDTO User { get; set; } = null!;
    }
}
