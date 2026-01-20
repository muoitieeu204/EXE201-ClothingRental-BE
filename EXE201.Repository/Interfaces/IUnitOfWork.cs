using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        public IUserRepository Users { get; }

        public IWishlistRepository Wishlists { get; }
        public IStudioRepository Studios { get; }
        public IServicePackageRepository ServicePackages { get; }
        public IServiceBookingRepository ServiceBookings { get; }
        public IServiceBookingAddonRepository ServiceBookingAddons { get; }
        public IRoleRepository Roles { get; }
        public IReviewRepository Reviews { get; }
        public IReviewImageRepository ReviewImages { get; }
        public IRentalPackageRepository RentalPackages { get; }
        public IPaymentRepository Payments { get; }
        public IOutfitSizeRepository OutfitSizes { get; }
        public IOutfitRepository Outfits { get; }
        public IOutfitImageRepository OutfitImages { get; }
        public IOutfitAttributeRepository OutfitAttributes { get; }
        public ILoyaltyTransactionRepository LoyaltyTransactions { get; }
        public IDepositTransactionRepository DepositTransactions { get; }
        public ICategoryRepository Categories { get; }
        public IBookingRepository Bookings { get; }
        public IBookingDetailRepository BookingDetails { get; }
        Task<int> SaveChangesAsync();
    }
}
