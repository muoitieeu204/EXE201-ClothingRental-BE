using EXE201.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IWishlistService
    {
        public Task<IEnumerable<WishlistDTO>> getAllAsync();
        public Task<WishlistDTO> getByIdAsync(int id);
        public Task<WishlistDTO> createAsync(WishlistDTO entity);
        public Task<WishlistDTO> updateAsync(int id, WishlistDTO entity);
        public Task<bool> deleteAsync(int id);
    }
}
