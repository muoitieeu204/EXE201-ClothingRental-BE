using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.AddressDTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class AddressService : IAddressService
    {
        private const string ACTIVE = "Active";
        private const string INACTIVE = "Inactive";

        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AddressService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        private static bool IsActive(string? status)
            => string.Equals(status?.Trim(), ACTIVE, StringComparison.OrdinalIgnoreCase);

        private static bool IsInactive(string? status)
            => string.Equals(status?.Trim(), INACTIVE, StringComparison.OrdinalIgnoreCase);

        public async Task<IEnumerable<AddressDto>> GetMyAddressesAsync(int userId, bool includeInactive = false)
        {
            var list = await _uow.Addresses.GetByUserIdAsync(userId);

            if (!includeInactive)
                list = list.Where(a => IsActive(a.Status));

            return _mapper.Map<IEnumerable<AddressDto>>(list);
        }

        public async Task<AddressDto?> GetMyAddressByIdAsync(int userId, int addressId)
        {
            var address = await _uow.Addresses.GetByIdAsync(addressId);
            if (address == null) return null;

            if (address.UserId != userId) return null;

            // nếu muốn chỉ cho lấy Active:
            // if (!IsActive(address.Status)) return null;

            return _mapper.Map<AddressDto>(address);
        }

        public async Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(int userId, bool includeInactive = false)
        {
            var list = await _uow.Addresses.GetByUserIdAsync(userId);

            if (!includeInactive)
                list = list.Where(a => IsActive(a.Status));

            return _mapper.Map<IEnumerable<AddressDto>>(list);
        }

        public async Task<AddressDto> CreateMyAddressAsync(int userId, CreateAddressDto dto)
        {
            var entity = _mapper.Map<Address>(dto);
            entity.UserId = userId;

            entity.Status = ACTIVE;

            await _uow.Addresses.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<AddressDto>(entity);
        }

        public async Task<bool> UpdateMyAddressAsync(int userId, int addressId, UpdateAddressDto dto)
        {
            var address = await _uow.Addresses.GetByIdAsync(addressId);
            if (address == null) return false;

            if (address.UserId != userId) return false;

            // chặn update address đã Inactive:
            if (IsInactive(address.Status)) return false;

            address.AddressText = dto.AddressText;

            await _uow.Addresses.UpdateAsync(address);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMyAddressAsync(int userId, int addressId)
        {
            var address = await _uow.Addresses.GetByIdAsync(addressId);
            if (address == null) return false;

            if (address.UserId != userId) return false;

            // soft delete
            if (IsInactive(address.Status)) return true;

            address.Status = INACTIVE;

            await _uow.Addresses.UpdateAsync(address);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreMyAddressAsync(int userId, int addressId)
        {
            var address = await _uow.Addresses.GetByIdAsync(addressId);
            if (address == null) return false;

            if (address.UserId != userId) return false;

            if (IsActive(address.Status)) return true;

            address.Status = ACTIVE;

            await _uow.Addresses.UpdateAsync(address);
            await _uow.SaveChangesAsync();
            return true;
        }
    }

}
