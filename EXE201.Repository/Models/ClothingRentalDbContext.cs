using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EXE201.Repository.Models;

public partial class ClothingRentalDbContext : DbContext
{
    public ClothingRentalDbContext()
    {
    }

    public ClothingRentalDbContext(DbContextOptions<ClothingRentalDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingDetail> BookingDetails { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<DepositTransaction> DepositTransactions { get; set; }

    public virtual DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }

    public virtual DbSet<Outfit> Outfits { get; set; }

    public virtual DbSet<OutfitAttribute> OutfitAttributes { get; set; }

    public virtual DbSet<OutfitImage> OutfitImages { get; set; }

    public virtual DbSet<OutfitSize> OutfitSizes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<RentalPackage> RentalPackages { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ReviewImage> ReviewImages { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ServiceAddon> ServiceAddons { get; set; }

    public virtual DbSet<ServiceBooking> ServiceBookings { get; set; }

    public virtual DbSet<ServiceBookingAddon> ServiceBookingAddons { get; set; }

    public virtual DbSet<ServicePackage> ServicePackages { get; set; }

    public virtual DbSet<Studio> Studios { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951AEDED75F7CB");

            entity.Property(e => e.BookingDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalDepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalRentalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalSurcharge).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Users");
        });

        modelBuilder.Entity<BookingDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__BookingD__135C316DBE6EF32B");

            entity.Property(e => e.DamageFee)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.LateFee)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingDetails_Bookings");

            entity.HasOne(d => d.OutfitSize).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.OutfitSizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingDetails_Sizes");

            entity.HasOne(d => d.RentalPackage).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.RentalPackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingDetails_Packages");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BB597226F");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<DepositTransaction>(entity =>
        {
            entity.HasKey(e => e.TransId).HasName("PK__DepositT__9E5DDB3CE666798D");

            entity.Property(e => e.AmountDeducted).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AmountHeld).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AmountRefunded).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DeductionReason).HasMaxLength(255);
            entity.Property(e => e.ProcessedAt).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Booking).WithMany(p => p.DepositTransactions)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deposits_Bookings");
        });

        modelBuilder.Entity<LoyaltyTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__LoyaltyT__55433A6BE108C940");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.LoyaltyTransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Loyalty_Users");
        });

        modelBuilder.Entity<Outfit>(entity =>
        {
            entity.HasKey(e => e.OutfitId).HasName("PK__Outfits__399B99B14F383719");

            entity.Property(e => e.BaseRentalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.IsLimited).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Outfits)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Outfits_Categories");
        });

        modelBuilder.Entity<OutfitAttribute>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__OutfitAt__135C316DE2B02E36");

            entity.HasIndex(e => e.OutfitId, "UQ__OutfitAt__399B99B0FE37C6B8").IsUnique();

            entity.Property(e => e.ColorPrimary).HasMaxLength(100);
            entity.Property(e => e.CulturalOrigin).HasMaxLength(255);
            entity.Property(e => e.FormalityLevel).HasMaxLength(100);
            entity.Property(e => e.Material).HasMaxLength(255);
            entity.Property(e => e.Occasion).HasMaxLength(255);
            entity.Property(e => e.SeasonSuitability).HasMaxLength(100);
            entity.Property(e => e.Silhouette).HasMaxLength(255);
            entity.Property(e => e.StoryTitle).HasMaxLength(255);

            entity.HasOne(d => d.Outfit).WithOne(p => p.OutfitAttribute)
                .HasForeignKey<OutfitAttribute>(d => d.OutfitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attributes_Outfits");
        });

        modelBuilder.Entity<OutfitImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__OutfitIm__7516F70C3E5B4D99");

            entity.Property(e => e.ImageType).HasMaxLength(50);

            entity.HasOne(d => d.Outfit).WithMany(p => p.OutfitImages)
                .HasForeignKey(d => d.OutfitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Images_Outfits");
        });

        modelBuilder.Entity<OutfitSize>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PK__OutfitSi__83BD097A7FE9850D");

            entity.Property(e => e.SizeLabel).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);

            entity.HasOne(d => d.Outfit).WithMany(p => p.OutfitSizes)
                .HasForeignKey(d => d.OutfitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sizes_Outfits");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38DFC019BE");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransactionRef).HasMaxLength(100);

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Bookings");
        });

        modelBuilder.Entity<RentalPackage>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__RentalPa__322035CC759D7CF4");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.OverdueFeePerHour).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CE43DCD86C");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Outfit).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.OutfitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Outfits");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Users");
        });

        modelBuilder.Entity<ReviewImage>(entity =>
        {
            entity.HasKey(e => e.ImgId).HasName("PK__ReviewIm__352F54F32A223C05");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewImages)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReviewImages_Reviews");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A9DDE009D");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<ServiceAddon>(entity =>
        {
            entity.HasKey(e => e.AddonId).HasName("PK__ServiceA__74289533E17AE3A0");

            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ServicePkg).WithMany(p => p.ServiceAddons)
                .HasForeignKey(d => d.ServicePkgId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Addons_Services");
        });

        modelBuilder.Entity<ServiceBooking>(entity =>
        {
            entity.HasKey(e => e.SvcBookingId).HasName("PK__ServiceB__05C39E96729E647E");

            entity.Property(e => e.ServiceTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Booking).WithMany(p => p.ServiceBookings)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_SvcBookings_ParentBooking");

            entity.HasOne(d => d.ServicePkg).WithMany(p => p.ServiceBookings)
                .HasForeignKey(d => d.ServicePkgId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SvcBookings_Pkg");

            entity.HasOne(d => d.User).WithMany(p => p.ServiceBookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SvcBookings_Users");
        });

        modelBuilder.Entity<ServiceBookingAddon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceB__3214EC0742035C39");

            entity.Property(e => e.PriceAtBooking).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Addon).WithMany(p => p.ServiceBookingAddons)
                .HasForeignKey(d => d.AddonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SvcDetails_Addon");

            entity.HasOne(d => d.SvcBooking).WithMany(p => p.ServiceBookingAddons)
                .HasForeignKey(d => d.SvcBookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SvcDetails_Booking");
        });

        modelBuilder.Entity<ServicePackage>(entity =>
        {
            entity.HasKey(e => e.ServicePkgId).HasName("PK__ServiceP__97092E973C5B42CF");

            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Studio).WithMany(p => p.ServicePackages)
                .HasForeignKey(d => d.StudioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Services_Studios");
        });

        modelBuilder.Entity<Studio>(entity =>
        {
            entity.HasKey(e => e.StudioId).HasName("PK__Studios__4ACC3B7078F83D25");

            entity.Property(e => e.ContactInfo).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CD82BB23B");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053421E2E02E").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.WishlistId).HasName("PK__Wishlist__233189EB42BBE89C");

            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Outfit).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.OutfitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Wishlists_Outfits");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Wishlists_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
