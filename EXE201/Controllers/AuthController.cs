using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDTO>> Register(CreateUserDTO request)
        {
            var result = await _authService.RegisterAsync(request);
            if (result is null) 
                return BadRequest(new { message = "User with this email already exists" });
  
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginDTO request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null) 
                return BadRequest(new { message = "Invalid email or password" });
      
            return Ok(result);
        }
    }
}