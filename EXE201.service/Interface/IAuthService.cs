using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IAuthService
    {
        Task<RegisterResponseDTO?> RegisterAsync(CreateUserDTO request);
        Task<LoginResponseDTO?> LoginAsync(LoginDTO request);
    }
}
