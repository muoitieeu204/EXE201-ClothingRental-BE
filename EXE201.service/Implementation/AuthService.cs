using Azure.Core;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using EXE201.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EXE201.Service.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginDTO request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return null;
            }

            var passwordHasher = new PasswordHasher<User>();
            var verifyPassword = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verifyPassword == PasswordVerificationResult.Failed)
            {
                return null;
            }

            string token = CreateToken(user);

            var response = new LoginResponseDTO
            {
                Token = token,
                User = new UserDTO
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    RoleId = user.RoleId,
                    RoleName = user.Role?.RoleName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            };

            return response;
        }

        public async Task<RegisterResponseDTO?> RegisterAsync(CreateUserDTO request)
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return null;
            }

            var passwordHasher = new PasswordHasher<User>();
            var hashedPassword = passwordHasher.HashPassword(null, request.Password);

            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = hashedPassword,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                RoleId = 3,  // Default role for new users (customer)
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            var response = new RegisterResponseDTO
            {
                UserId = newUser.UserId,
                Email = newUser.Email,
                FullName = newUser.FullName,
                PhoneNumber = newUser.PhoneNumber,
                RoleId = newUser.RoleId,
                Message = "User registered successfully"
            };

            return response;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
        {
             new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
             new Claim(ClaimTypes.Email, user.Email),
             new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
             new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new JwtSecurityToken(
             issuer: _configuration["AppSettings:Issuer"],
             audience: _configuration["AppSettings:Audience"],
             claims: claims,
             expires: DateTime.UtcNow.AddDays(1),
             signingCredentials: creds
           );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
