using EXE201.Repository.Interfaces;
using EXE201.Service.DTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class UserService : IUserService
    {
    private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
 _unitOfWork = unitOfWork;
}

     public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
    {
  var users = await _unitOfWork.Users.GetAllUserAsync();
     
            return users.Select(u => new UserDTO
            {
        UserId = u.UserId,
    Email = u.Email,
          FullName = u.FullName,
            PhoneNumber = u.PhoneNumber,
     AvatarUrl = u.AvatarUrl,
     RoleId = u.RoleId,
      RoleName = u.Role?.RoleName,
              IsActive = u.IsActive,
     CreatedAt = u.CreatedAt,
      UpdatedAt = u.UpdatedAt
 }).ToList();
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
     if (user == null) return null;

            return new UserDTO
            {
                UserId = user.UserId,
      Email = user.Email,
       FullName = user.FullName,
      PhoneNumber = user.PhoneNumber,
     AvatarUrl = user.AvatarUrl,
    RoleId = user.RoleId,
        RoleName = user.Role?.RoleName,
   IsActive = user.IsActive,
             CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
        };
      }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null) return null;

   return new UserDTO
            {
    UserId = user.UserId,
            Email = user.Email,
   FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
 AvatarUrl = user.AvatarUrl,
         RoleId = user.RoleId,
    RoleName = user.Role?.RoleName,
     IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
      UpdatedAt = user.UpdatedAt
            };
        }
    }
}
