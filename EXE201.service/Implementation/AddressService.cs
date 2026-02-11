using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.AddressDTOs;
using EXE201.Service.Interface;

namespace EXE201.Service.Implementation
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AddressService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AddressDto>> GetMyAddressesAsync(int userId)
        {
            var list = await _uow.UserAddresses.GetByUserIdAsync(userId);

            // thường UI muốn default lên đầu
            list = list
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.AddressId);

            return _mapper.Map<IEnumerable<AddressDto>>(list);
        }

        public async Task<AddressDto?> GetMyAddressByIdAsync(int userId, int addressId)
        {
            var address = await _uow.UserAddresses.GetByIdAsync(addressId);
            if (address == null) return null;

            if (address.UserId != userId) return null;

            return _mapper.Map<AddressDto>(address);
        }

        public async Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(int userId)
        {
            var list = await _uow.UserAddresses.GetByUserIdAsync(userId);

            list = list
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.AddressId);

            return _mapper.Map<IEnumerable<AddressDto>>(list);
        }

        public async Task<AddressDto> CreateMyAddressAsync(int userId, CreateAddressDto dto)
        {
            var entity = _mapper.Map<UserAddress>(dto);
            entity.UserId = userId;

            // nếu user chưa có address nào => auto default cho đỡ mệt
            var current = await _uow.UserAddresses.GetByUserIdAsync(userId);
            var hasAny = current.Any();

            if (!hasAny)
            {
                entity.IsDefault = true;
            }
            else if (entity.IsDefault == true)
            {
                await UnsetDefaultOthersAsync(userId, keepAddressId: null);
            }

            await _uow.UserAddresses.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return _mapper.Map<AddressDto>(entity);
        }

        public async Task<bool> UpdateMyAddressAsync(int userId, int addressId, UpdateAddressDto dto)
        {
            var address = await _uow.UserAddresses.GetByIdAsync(addressId);
            if (address == null) return false;

            if (address.UserId != userId) return false;

            _mapper.Map(dto, address);

            // nếu bật IsDefault => tắt default của các address khác
            if (address.IsDefault == true)
            {
                await UnsetDefaultOthersAsync(userId, keepAddressId: address.AddressId);
            }

            await _uow.UserAddresses.UpdateAsync(address);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMyAddressAsync(int userId, int addressId)
        {
            var address = await _uow.UserAddresses.GetByIdAsync(addressId);
            if (address == null) return false;

            if (address.UserId != userId) return false;

            var wasDefault = address.IsDefault == true;

            // (1) Gỡ liên kết Booking.AddressId trước (set null)
            var bookings = await _uow.Bookings.GetBookingsByAddressIdAsync(addressId);
            if (bookings.Count > 0)
            {
                foreach (var b in bookings)
                {
                    b.AddressId = null;
                    await _uow.Bookings.UpdateAsync(b);
                }
                await _uow.SaveChangesAsync();
            }

            // (2) Xoá address (hard delete)
            await _uow.UserAddresses.DeleteAsync(address);
            await _uow.SaveChangesAsync();

            // (3) Nếu xoá cái default => set 1 cái còn lại thành default (nếu còn)
            if (wasDefault)
            {
                var remain = await _uow.UserAddresses.GetByUserIdAsync(userId);
                var pick = remain.OrderByDescending(a => a.AddressId).FirstOrDefault();

                if (pick != null && pick.IsDefault != true)
                {
                    pick.IsDefault = true;
                    await _uow.UserAddresses.UpdateAsync(pick);
                    await _uow.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<AddressDto?> SetMyDefaultAddressAsync(int userId, int addressId)
        {
            var address = await _uow.UserAddresses.GetByIdAsync(addressId);
            if (address == null) return null;
            if (address.UserId != userId) return null;

            var list = await _uow.UserAddresses.GetByUserIdAsync(userId);

            foreach (var a in list)
            {
                var shouldBeDefault = a.AddressId == addressId;

                // bool? safe compare
                if ((a.IsDefault == true) != shouldBeDefault)
                {
                    a.IsDefault = shouldBeDefault;
                    await _uow.UserAddresses.UpdateAsync(a);
                }
            }

            await _uow.SaveChangesAsync();

            // trả về đúng addressId vừa set (đã IsDefault = true)
            // (address object cũng đã được update trong loop nếu nó nằm trong list)
            return _mapper.Map<AddressDto>(address);
        }



        /// <summary>
        /// Tắt IsDefault của các address khác (để tránh 2 default).
        /// </summary>
        private async Task UnsetDefaultOthersAsync(int userId, int? keepAddressId)
        {
            var list = await _uow.UserAddresses.GetByUserIdAsync(userId);

            var others = list.Where(a =>
                a.IsDefault == true &&
                (!keepAddressId.HasValue || a.AddressId != keepAddressId.Value));

            foreach (var a in others)
            {
                a.IsDefault = false;
                await _uow.UserAddresses.UpdateAsync(a);
            }
        }

    }
}
