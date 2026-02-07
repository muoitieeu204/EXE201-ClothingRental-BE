using EXE201.Service.DTOs.AddressDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetMyAddressesAsync(int userId);
        Task<AddressDto?> GetMyAddressByIdAsync(int userId, int addressId);
        Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(int userId);

        Task<AddressDto> CreateMyAddressAsync(int userId, CreateAddressDto dto);
        Task<bool> UpdateMyAddressAsync(int userId, int addressId, UpdateAddressDto dto);
        Task<bool> DeleteMyAddressAsync(int userId, int addressId);
    }
}
