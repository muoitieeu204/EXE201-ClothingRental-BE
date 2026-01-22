using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.DTOs.UserDTOs
{
    public class UpdateUserProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class SendChangePasswordOtpDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordWithOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
