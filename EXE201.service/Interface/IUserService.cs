using EXE201.Service.DTOs;
using EXE201.Service.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<ListUserDto>> GetAllAsync();
        Task<UserDetailDto?> GetByIdAsync(int id);
        Task<bool> SoftDeleteAsync(int id); // Update IsActive to false
        Task<UserDetailDto?> UpdateProfileAsync(int id, UpdateUserProfileDto dto);

        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(int userId);
        Task<UserDTO?> GetUserByEmailAsync(string email);

        Task<bool> SendChangePasswordOtpAsync(string email);
        Task<bool> ChangePasswordLoggedInAsync(int userId, ChangePasswordNewOnlyDto dto);

        Task<string?> VerifyChangePasswordOtpAsync(string email, string otp);
        Task<bool> ResetPasswordAfterOtpAsync(ResetPasswordAfterOtpDto dto);
    }
}
