using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Service.DTOs;
using EXE201.Service.DTOs.UserDTOs;
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
        private readonly IUnitOfWork _uow;
        private readonly IMapper Mapper;

        public UserService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            Mapper = mapper;
        }

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

            // soft delete = deactivate
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow; // hoặc DateTime.Now nếu bạn muốn theo local

            await _uow.Users.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            return true;
        }

        public async Task<UserDetailDto?> UpdateProfileAsync(int id, UpdateUserProfileDto dto)
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user == null) return null;

            if (user.IsActive == false) return null; // optional: không cho edit nếu account đã deactivate

            // Map dto -> entity (chỉ update field != null)
            Mapper.Map(dto, user);

            user.UpdatedAt = DateTime.UtcNow;

            await _uow.Users.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            return Mapper.Map<UserDetailDto>(user);
        }
    }
}
