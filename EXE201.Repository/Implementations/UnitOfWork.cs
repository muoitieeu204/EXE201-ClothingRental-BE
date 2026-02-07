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
        private IOutfitAttributeRepository _outfitAttribute;
        private IRentalPackageRepository _rentalPackage;
        private IBookingRepository _booking;
        private IBookingDetailRepository _bookingDetail;
        private IPaymentRepository _payment;
        private IDepositTransactionRepository _depositTransaction;
        private IServicePackageRepository _servicePackage;
        private IServiceAddonRepository _serviceAddon;
        private IServiceBookingRepository _serviceBooking;
        private IServiceBookingAddonRepository _serviceBookingAddon;
        private IAddressRepository _address;


        public UnitOfWork(ClothingRentalDbContext context)
        {
            _context = context;
        }
        public IAddressRepository UserAddresses => _address ??= new AddressRepository(_context);

        public IUserRepository Users => _user ??= new UserRepository(_context);

        public IWishlistRepository Wishlists => _wishlist ??= new WishlistRepository(_context);

        public IStudioRepository Studios => throw new NotImplementedException();

        public IServicePackageRepository ServicePackages => _servicePackage ??= new ServicePackageRepository(_context);

        public IServiceBookingRepository ServiceBookings => _serviceBooking ??= new ServiceBookingRepository(_context);

        public IServiceBookingAddonRepository ServiceBookingAddons => _serviceBookingAddon ??= new ServiceBookingAddonRepository(_context);

        public IServiceAddonRepository ServiceAddons => _serviceAddon ??= new ServiceAddonRepository(_context);

        public IRoleRepository Roles => throw new NotImplementedException();

        public IReviewRepository Reviews => throw new NotImplementedException();

        public IReviewImageRepository ReviewImages => throw new NotImplementedException();

        public IRentalPackageRepository RentalPackages => _rentalPackage ??= new RentalPackageRepository(_context);

        public IPaymentRepository Payments => _payment ??= new PaymentRepository(_context);

        public IOutfitSizeRepository OutfitSizes => _outfitSize ??= new OutfitSizeRepository(_context);

        public IOutfitRepository Outfits => _outfit ??= new OutfitRepository(_context);

        public IOutfitImageRepository OutfitImages => _outfitImage ??= new OutfitImageRepository(_context);

        public IOutfitAttributeRepository OutfitAttributes => _outfitAttribute ??= new OutfitAttributeRepository(_context);
        public ILoyaltyTransactionRepository LoyaltyTransactions => throw new NotImplementedException();

        public IDepositTransactionRepository DepositTransactions => _depositTransaction ??= new DepositTransactionRepository(_context);

        public ICategoryRepository Categories => _category ??= new CategoryRepository(_context);
        public IBookingRepository Bookings => _booking ??= new BookingRepository(_context);

        public IBookingDetailRepository BookingDetails => _bookingDetail ??= new BookingDetailRepository(_context);

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
