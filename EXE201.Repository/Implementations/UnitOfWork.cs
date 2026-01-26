using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Repository.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClothingRentalDbContext _context;
        private IUserRepository _user;
        private IWishlistRepository _wishlist;
        private IOutfitRepository _outfit;
        private IOutfitImageRepository _outfitImage;
        private IOutfitSizeRepository _outfitSize;
        private ICategoryRepository _category;

        public UnitOfWork(ClothingRentalDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _user ??= new UserRepository(_context);

        public IWishlistRepository Wishlists => _wishlist ??= new WishlistRepository(_context);

        public IStudioRepository Studios => throw new NotImplementedException();

        public IServicePackageRepository ServicePackages => throw new NotImplementedException();

        public IServiceBookingRepository ServiceBookings => throw new NotImplementedException();

        public IServiceBookingAddonRepository ServiceBookingAddons => throw new NotImplementedException();

        public IRoleRepository Roles => throw new NotImplementedException();

        public IReviewRepository Reviews => throw new NotImplementedException();

        public IReviewImageRepository ReviewImages => throw new NotImplementedException();

        public IRentalPackageRepository RentalPackages => throw new NotImplementedException();

        public IPaymentRepository Payments => throw new NotImplementedException();

        public IOutfitSizeRepository OutfitSizes => _outfitSize ??= new OutfitSizeRepository(_context);

        public IOutfitRepository Outfits => _outfit ??= new OutfitRepository(_context);

        public IOutfitImageRepository OutfitImages => _outfitImage ??= new OutfitImageRepository(_context);

        public IOutfitAttributeRepository OutfitAttributes => throw new NotImplementedException();

        public ILoyaltyTransactionRepository LoyaltyTransactions => throw new NotImplementedException();

        public IDepositTransactionRepository DepositTransactions => throw new NotImplementedException();

        public ICategoryRepository Categories => _category ??= new CategoryRepository(_context);
        public IBookingRepository Bookings => throw new NotImplementedException();

        public IBookingDetailRepository BookingDetails => throw new NotImplementedException();

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
