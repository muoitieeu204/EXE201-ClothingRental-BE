using AutoMapper;
using EXE201.Repository.Implementations;
using EXE201.Service.DTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class WishlistService : IWishlistService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public WishlistService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }
        public async Task<WishlistDTO> createAsync(WishlistDTO entity)
        {
            var wishlist = _unitOfWork.Wishlists.AddAsync(entity);
        }

        public Task<bool> deleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WishlistDTO>> getAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<WishlistDTO> getByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<WishlistDTO> updateAsync(int id, WishlistDTO entity)
        {
            throw new NotImplementedException();
        }
    }
}
