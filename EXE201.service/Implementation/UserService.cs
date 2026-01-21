using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Service.DTOs;
using EXE201.Service.DTOs.UserDTOs;
using EXE201.Service.Interface;

namespace EXE201.Service.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper Mapper;

        public UserService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            Mapper = mapper;
        }

        // ====== Bộ mới ======

        public async Task<IEnumerable<ListUserDto>> GetAllAsync()
        {
            var users = await _uow.Users.GetAllUserAsync();
            return Mapper.Map<IEnumerable<ListUserDto>>(users);
        }

        public async Task<UserDetailDto?> GetByIdAsync(int id)
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user == null) return null;

            return Mapper.Map<UserDetailDto>(user);
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
    }
}
