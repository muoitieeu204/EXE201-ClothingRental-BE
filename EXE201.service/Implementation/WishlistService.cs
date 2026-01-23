using AutoMapper;
using EXE201.Repository.Implementations;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public WishlistService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
   
        public async Task<IEnumerable<WishlistDTO>> GetWishlistsByUserIdAsync(int userId)
        {
            var wishlists = await _unitOfWork.Wishlists.FindAsync(w => w.UserId == userId);
            return _mapper.Map<IEnumerable<WishlistDTO>>(wishlists);
        }

        public async Task<WishlistDTO> GetByIdAsync(int id)
        {
            var wishlist = await _unitOfWork.Wishlists.GetByIdAsync(id);
            if (wishlist == null) return null;
            return _mapper.Map<WishlistDTO>(wishlist);
        }

        public async Task<bool> AddToWishlistAsync(int userId, int outfitId)
        {
            // Check if already exists
            var exists = await IsInWishlistAsync(userId, outfitId);
            if (exists) return false; // Already in wishlist

            var wishlist = new Wishlist
            {
                UserId = userId,
                OutfitId = outfitId,
                AddedAt = DateTime.UtcNow
            };

            await _unitOfWork.Wishlists.AddAsync(wishlist);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromWishlistAsync(int wishlistId, int userId)
        {
            var wishlist = await _unitOfWork.Wishlists.GetByIdAsync(wishlistId);
            
            // Verify ownership
            if (wishlist == null || wishlist.UserId != userId) 
                return false;

            await _unitOfWork.Wishlists.DeleteAsync(wishlist);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsInWishlistAsync(int userId, int outfitId)
        {
            return await _unitOfWork.Wishlists.ExistAsync(w => 
                w.UserId == userId && w.OutfitId == outfitId);
        }
    }
}
