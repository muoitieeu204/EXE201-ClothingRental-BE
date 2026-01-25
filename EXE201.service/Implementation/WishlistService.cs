using AutoMapper;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.DTOs.WishlistDTOs;
using EXE201.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
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
   
        public async Task<IEnumerable<WishlistResponseDto>> GetWishlistsByUserIdAsync(int userId)
        {
            var wishlists = await _unitOfWork.Wishlists.FindAsync(w => w.UserId == userId);
            return _mapper.Map<IEnumerable<WishlistResponseDto>>(wishlists);
        }

        public async Task<WishlistResponseDto?> GetByIdAsync(int id)
        {
            var wishlist = await _unitOfWork.Wishlists.GetByIdAsync(id);
            if (wishlist == null) return null;
            return _mapper.Map<WishlistResponseDto>(wishlist);
        }

        public async Task<bool> AddToWishlistAsync(int userId, AddToWishlistDto dto)
        {
            // Check if already exists
            var checkResult = await IsInWishlistAsync(userId, dto.OutfitId);
            if (checkResult.IsInWishlist) return false; // Already in wishlist

            var wishlist = new Wishlist
            {
                UserId = userId,
                OutfitId = dto.OutfitId,
                AddedAt = DateTime.UtcNow
            };

            await _unitOfWork.Wishlists.AddAsync(wishlist);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int outfitId)
        {
            var wishlist = await _unitOfWork.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.OutfitId == outfitId);
            
            // Verify ownership
            if (wishlist == null) 
                return false;

            await _unitOfWork.Wishlists.DeleteAsync(wishlist);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<WishlistCheckDto> IsInWishlistAsync(int userId, int outfitId)
        {
            var wishlist = await _unitOfWork.Wishlists.FirstOrDefaultAsync(w => 
                w.UserId == userId && w.OutfitId == outfitId);
            
            return new WishlistCheckDto
            {
                IsInWishlist = wishlist != null,
                WishlistId = wishlist?.WishlistId
            };
        }
    }
}
