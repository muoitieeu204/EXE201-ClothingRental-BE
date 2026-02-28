using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Service.DTOs;
using EXE201.Service.DTOs.UserDTOs;
using EXE201.Service.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;


namespace EXE201.Service.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper Mapper;

        private readonly IMemoryCache? _cache;
        private readonly IEmailService? _emailService;
        private readonly IConfiguration? _config;
        public UserService(
            IUnitOfWork uow,
            IMapper mapper,
            IMemoryCache? cache = null,
            IEmailService? emailService = null,
            IConfiguration? config = null
        )
        {
            _uow = uow;
            Mapper = mapper;

            _cache = cache;
            _emailService = emailService;
            _config = config;
        }


        // ====== Bộ mới ======

        public async Task<IEnumerable<ListUserDto>> GetAllAsync()
        {
            var users = await _uow.Users.GetAllUserAsync();
            var userDtos = new List<ListUserDto>();

            foreach (var user in users)
            {
                var dto = Mapper.Map<ListUserDto>(user);
                // Count total bookings for this user
                dto.TotalOrders = await _uow.Bookings.CountByUserIdAsync(user.UserId);
                userDtos.Add(dto);
            }

            return userDtos;
        }

        public async Task<UserDetailDto?> GetByIdAsync(int id)
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user == null) return null;

            var dto = Mapper.Map<UserDetailDto>(user);
            // Count total bookings for this user
            dto.TotalOrders = await _uow.Bookings.CountByUserIdAsync(user.UserId);

            return dto;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _uow.Users.UpdateAsync(user);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<UserDetailDto?> UpdateProfileAsync(int id, UpdateUserProfileDto dto)
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user == null) return null;

            if (user.IsActive == false) return null;

            // AutoMapper: chỉ update field != null (nếu bạn config như mình nói)
            Mapper.Map(dto, user);

            user.UpdatedAt = DateTime.UtcNow;

            await _uow.Users.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            return Mapper.Map<UserDetailDto>(user);
        }

        // ====== Bộ cũ (giữ để khỏi break code cũ) ======

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _uow.Users.GetAllUserAsync();
            return Mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null) return null;

            return Mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            var user = await _uow.Users.GetByEmailAsync(email);
            if (user == null) return null;

            return Mapper.Map<UserDTO>(user);
        }

        //===================================================
        private sealed class OtpCacheItem
        {
            public string OtpHash { get; set; } = string.Empty;
            public DateTime ExpireAtUtc { get; set; }
            public int AttemptsLeft { get; set; }
        }

        private string NormalizeEmail(string email) => (email ?? "").Trim().ToLowerInvariant();

        private int GetIntConfig(string key, int fallback)
        {
            var s = _config?[key];
            return int.TryParse(s, out var v) ? v : fallback;
        }

        private string GetOtpSecret()
        {
            // Nếu bạn quên set Secret thì vẫn chạy demo, nhưng nhớ set thật khi deploy
            return _config?["Otp:Secret"] ?? "DEV_ONLY_SECRET_CHANGE_ME";
        }

        private string OtpKey(string email) => $"OTP:CHANGE_PASSWORD:{NormalizeEmail(email)}";
        private string CooldownKey(string email) => $"OTP:COOLDOWN:{NormalizeEmail(email)}";

        private static string GenerateOtp6Digits()
        {
            // 000000 - 999999 (crypto RNG)
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt32(bytes);
            var otp = (value % 1_000_000).ToString("D6");
            return otp;
        }

        private string HashOtp(string email, string otp)
        {
            // HMAC(email + otp) để khỏi lưu OTP plaintext
            var secret = Encoding.UTF8.GetBytes(GetOtpSecret());
            using var hmac = new HMACSHA256(secret);

            var payload = Encoding.UTF8.GetBytes($"{NormalizeEmail(email)}|{otp}");
            var hash = hmac.ComputeHash(payload);
            return Convert.ToBase64String(hash);
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            var ba = Encoding.UTF8.GetBytes(a ?? "");
            var bb = Encoding.UTF8.GetBytes(b ?? "");
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }

        // Setter password hash bằng reflection để bạn khỏi kẹt tên field (Password vs PasswordHash)
        private static void SetUserPasswordHash(object userEntity, string hashedPassword)
        {
            var t = userEntity.GetType();
            var prop = t.GetProperty("PasswordHash") ?? t.GetProperty("Password");
            if (prop == null || !prop.CanWrite)
                throw new InvalidOperationException("Không tìm thấy field PasswordHash hoặc Password để set mật khẩu.");

            prop.SetValue(userEntity, hashedPassword);
        }

        // === Bạn PHẢI đổi hàm hash này cho khớp với hệ login hiện tại của bạn ===
        // Nếu project bạn đang dùng BCrypt ở login thì đổi sang BCrypt cho đồng bộ.
        private static string HashPassword_Pbkdf2(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password trống.");

            const int iterations = 100_000;
            Span<byte> salt = stackalloc byte[16];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt.ToArray(), iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            return $"PBKDF2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public async Task<bool> SendChangePasswordOtpAsync(string email)
        {
            if (_cache == null || _emailService == null)
                throw new InvalidOperationException("Thiếu IMemoryCache hoặc IEmailService trong DI.");

            email = NormalizeEmail(email);
            if (string.IsNullOrWhiteSpace(email)) return false;

            // optional: chống spam resend
            var cooldownSeconds = GetIntConfig("Otp:ResendCooldownSeconds", 30);
            if (_cache.TryGetValue(CooldownKey(email), out _))
            {
                return false; // đang cooldown
            }

            var user = await _uow.Users.GetByEmailAsync(email);
            if (user == null) return false;
            if (user.IsActive == false) return false;

            var ttlMinutes = GetIntConfig("Otp:TtlMinutes", 5);
            var maxAttempts = GetIntConfig("Otp:MaxAttempts", 5);

            var otp = GenerateOtp6Digits();
            var item = new OtpCacheItem
            {
                OtpHash = HashOtp(email, otp),
                ExpireAtUtc = DateTime.UtcNow.AddMinutes(ttlMinutes),
                AttemptsLeft = maxAttempts
            };

            _cache.Set(OtpKey(email), item, item.ExpireAtUtc);
            _cache.Set(CooldownKey(email), true, TimeSpan.FromSeconds(cooldownSeconds));

            var subject = "OTP đổi mật khẩu";
            var body =
                $@"Bạn vừa yêu cầu đổi mật khẩu.
                OTP của bạn là: {otp}
                OTP hết hạn sau {ttlMinutes} phút.

                Nếu không phải bạn, bỏ qua email này.";

            await _emailService.SendAsync(email, subject, body);
            return true;
        }

        public async Task<bool> ChangePasswordLoggedInAsync(int userId, ChangePasswordNewOnlyDto dto)
        {
            if (userId <= 0) return false;
            if (dto == null) return false;

            // ✅ bắt buộc mật khẩu hiện tại
            if (string.IsNullOrWhiteSpace(dto.OldPassword)) return false;

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8) return false;
            if (dto.NewPassword != dto.ConfirmPassword) return false;

            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null) return false;
            if (user.IsActive == false) return false;

            // đọc hash đang lưu
            var storedHash = GetUserPasswordHash(user);

            // verify mật khẩu hiện tại theo Identity
            var hasher = new PasswordHasher<object>();
            PasswordVerificationResult verifyResult;

            try
            {
                verifyResult = hasher.VerifyHashedPassword(user, storedHash, dto.OldPassword);
            }
            catch (FormatException)
            {
                // DB còn lẫn hash kiểu cũ như "123" => coi như sai
                return false;
            }

            if (verifyResult == PasswordVerificationResult.Failed) return false;

            // ✅ đổi pass: luôn hash theo Identity để login không crash
            var newHashed = HashPassword_Identity(user, dto.NewPassword);
            SetUserPasswordHash(user, newHashed);

            user.UpdatedAt = DateTime.UtcNow;
            await _uow.Users.UpdateAsync(user);
            await _uow.SaveChangesAsync();
            return true;
        }


        private sealed class ResetTokenCacheItem
        {
            public string Email { get; set; } = string.Empty;
            public DateTime ExpireAtUtc { get; set; }
        }

        private string ResetKey(string tokenHash) => $"RESET:CHANGE_PASSWORD:{tokenHash}";

        private static string Base64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Base64Url(bytes);
        }

        private string HashResetToken(string resetToken)
        {
            // HMAC(resetToken) để lưu cache dạng hash (khỏi lưu plaintext)
            var secret = Encoding.UTF8.GetBytes(GetOtpSecret());
            using var hmac = new HMACSHA256(secret);

            var payload = Encoding.UTF8.GetBytes($"RESET|{resetToken}");
            var hash = hmac.ComputeHash(payload);

            // base64 url-safe cho key cache
            return Base64Url(hash);
        }

        private static string GetUserPasswordHash(object userEntity)
        {
            var t = userEntity.GetType();
            var prop = t.GetProperty("PasswordHash") ?? t.GetProperty("Password");
            if (prop == null || !prop.CanRead)
                throw new InvalidOperationException("Không tìm thấy PasswordHash hoặc Password để đọc.");

            return prop.GetValue(userEntity)?.ToString() ?? "";
        }

        private static string HashPassword_Identity(object userEntity, string password)
        {
            // Hash kiểu ASP.NET Identity (chuỗi thường bắt đầu AQAAAA...)
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();
            return hasher.HashPassword(userEntity, password);
        }

        /// <summary>
        /// Verify OTP đúng -> trả resetToken (client giữ token này gọi API reset password)
        /// Sai/expired -> null
        /// </summary>
        public Task<string?> VerifyChangePasswordOtpAsync(string email, string otp)
        {
            if (_cache == null)
                throw new InvalidOperationException("Thiếu IMemoryCache trong DI.");

            email = NormalizeEmail(email);
            otp = (otp ?? "").Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
                return Task.FromResult<string?>(null);

            if (!_cache.TryGetValue(OtpKey(email), out OtpCacheItem? item) || item == null)
                return Task.FromResult<string?>(null);

            if (DateTime.UtcNow > item.ExpireAtUtc)
            {
                _cache.Remove(OtpKey(email));
                return Task.FromResult<string?>(null);
            }

            if (item.AttemptsLeft <= 0)
            {
                _cache.Remove(OtpKey(email));
                return Task.FromResult<string?>(null);
            }

            var inputHash = HashOtp(email, otp);
            if (!FixedTimeEquals(item.OtpHash, inputHash))
            {
                item.AttemptsLeft--;
                _cache.Set(OtpKey(email), item, item.ExpireAtUtc);
                return Task.FromResult<string?>(null);
            }

            // OTP đúng -> xoá OTP luôn để khỏi reuse
            _cache.Remove(OtpKey(email));

            // tạo resetToken 1 lần dùng
            var resetTtlMinutes = GetIntConfig("Otp:ResetTokenTtlMinutes", 10);
            var resetToken = GenerateSecureToken();
            var tokenHash = HashResetToken(resetToken);

            var resetItem = new ResetTokenCacheItem
            {
                Email = email,
                ExpireAtUtc = DateTime.UtcNow.AddMinutes(resetTtlMinutes)
            };

            _cache.Set(ResetKey(tokenHash), resetItem, resetItem.ExpireAtUtc);

            return Task.FromResult<string?>(resetToken);
        }

        /// <summary>
        /// Reset password sau khi đã verify OTP (không cần mật khẩu cũ) nhưng bắt buộc có resetToken.
        /// </summary>
        public async Task<bool> ResetPasswordAfterOtpAsync(ResetPasswordAfterOtpDto dto)
        {
            if (_cache == null)
                throw new InvalidOperationException("Thiếu IMemoryCache trong DI.");

            if (dto == null) return false;

            var email = NormalizeEmail(dto.Email);
            if (string.IsNullOrWhiteSpace(email)) return false;

            if (string.IsNullOrWhiteSpace(dto.ResetToken)) return false;

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
                return false;

            if (dto.NewPassword != dto.ConfirmPassword)
                return false;

            var tokenHash = HashResetToken(dto.ResetToken);

            if (!_cache.TryGetValue(ResetKey(tokenHash), out ResetTokenCacheItem? item) || item == null)
                return false;

            if (DateTime.UtcNow > item.ExpireAtUtc)
            {
                _cache.Remove(ResetKey(tokenHash));
                return false;
            }

            // token 1 lần dùng
            _cache.Remove(ResetKey(tokenHash));

            // đảm bảo token đúng email
            if (!string.Equals(item.Email, email, StringComparison.OrdinalIgnoreCase))
                return false;

            var user = await _uow.Users.GetByEmailAsync(email);
            if (user == null) return false;
            if (user.IsActive == false) return false;

            var newHashed = HashPassword_Identity(user, dto.NewPassword);
            SetUserPasswordHash(user, newHashed);

            user.UpdatedAt = DateTime.UtcNow;
            await _uow.Users.UpdateAsync(user);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
