using AutoMapper;
using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using EXE201.Service.DTOs.AddressDTOs;
using EXE201.Service.DTOs.BookingDTOs;
using EXE201.Service.DTOs.CategoryDTOs;
using EXE201.Service.DTOs.LoyaltyTransactionDTOs;
using EXE201.Service.DTOs.OutfitAttributeDTOs;
using EXE201.Service.DTOs.OutfitDTOs;
using EXE201.Service.DTOs.OutfitImageDTOs;
using EXE201.Service.DTOs.OutfitSizeDTOs;
using EXE201.Service.DTOs.RentalPackageDTOs;
using EXE201.Service.DTOs.ReviewDTOs;
using EXE201.Service.DTOs.ReviewImageDTOs;
using EXE201.Service.DTOs.ServiceBookingDTOs;
using EXE201.Service.DTOs.ServicePackageDTOs;
using EXE201.Service.DTOs.StudioDTOs;
using EXE201.Service.DTOs.UserDTOs;
using EXE201.Service.DTOs.WishlistDTOs;
using System.Linq;

namespace EXE201.Service.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // UserDTOs -> Model.User
            CreateMap<User, ListUserDto>().ReverseMap();
            CreateMap<User, UserDetailDto>().ReverseMap();
            CreateMap<UpdateUserProfileDto, User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<User, UserDTO>()
                .ForMember(opt => opt.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));

            // WishlistDTOs -> Model.Wishlist
            CreateMap<Wishlist, WishlistResponseDto>()
                .ForMember(dest => dest.OutfitName, opt => opt.MapFrom(src => src.Outfit != null ? src.Outfit.Name : null))
                .ForMember(dest => dest.OutfitPrice, opt => opt.MapFrom(src => src.Outfit != null ? (decimal?)src.Outfit.BaseRentalPrice : null))
                .ForMember(dest => dest.OutfitImageUrl, opt => opt.MapFrom(src =>
                    src.Outfit != null && src.Outfit.OutfitImages != null && src.Outfit.OutfitImages.Any()
                        ? src.Outfit.OutfitImages.OrderBy(img => img.SortOrder).FirstOrDefault()!.ImageUrl
                        : null));
            CreateMap<AddToWishlistDto, Wishlist>();

            // ServicePackageDTOs -> Model.ServicePackage
            CreateMap<ServicePackage, ServicePackageResponseDto>()
                .ForMember(dest => dest.StudioName, opt => opt.MapFrom(src => src.Studio != null ? src.Studio.Name : null))
                .ForMember(dest => dest.TotalAddons, opt => opt.MapFrom(src => src.ServiceAddons != null ? src.ServiceAddons.Count : 0))
                .ForMember(dest => dest.TotalBookings, opt => opt.MapFrom(src => src.ServiceBookings != null ? src.ServiceBookings.Count : 0));

            CreateMap<ServicePackage, ServicePackageDetailDto>()
                .ForMember(dest => dest.StudioName, opt => opt.MapFrom(src => src.Studio != null ? src.Studio.Name : null))
                .ForMember(dest => dest.StudioAddress, opt => opt.MapFrom(src => src.Studio != null ? src.Studio.Address : null))
                .ForMember(dest => dest.StudioContactInfo, opt => opt.MapFrom(src => src.Studio != null ? src.Studio.ContactInfo : null))
                .ForMember(dest => dest.Addons, opt => opt.MapFrom(src => src.ServiceAddons))
                .ForMember(dest => dest.TotalBookings, opt => opt.MapFrom(src => src.ServiceBookings != null ? src.ServiceBookings.Count : 0));

            CreateMap<ServiceAddon, ServiceAddonInfo>();
            CreateMap<CreateServicePackageDto, ServicePackage>();
            CreateMap<UpdateServicePackageDto, ServicePackage>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // OutfitImageDTOs -> Model.OutfitImage
            CreateMap<OutfitImage, OutfitImageResponseDto>().ReverseMap();
            CreateMap<CreateOutfitImageDto, OutfitImage>();
            CreateMap<UpdateOutfitImageDto, OutfitImage>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // OutfitSizeDTOs -> Model.OutfitSize
            CreateMap<OutfitSize, OutfitSizeResponseDto>()
                .ForMember(dest => dest.OutfitName, opt => opt.MapFrom(src => src.Outfit != null ? src.Outfit.Name : null));
            CreateMap<CreateOutfitSizeDto, OutfitSize>();
            CreateMap<UpdateOutfitSizeDto, OutfitSize>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ✅ OutfitAttributeDTOs -> Model.OutfitAttribute (BỔ SUNG ĐẦY ĐỦ)
            // dùng cho GetAll/GetById map entity -> OutfitAttributeDto
            CreateMap<OutfitAttribute, OutfitAttributeDto>().ReverseMap();

            // dùng cho Create
            CreateMap<CreateOutfitAttributeDto, OutfitAttribute>();

            // dùng cho Update (partial update - chỉ map field != null)
            CreateMap<UpdateOutfitAttributeDto, OutfitAttribute>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // dùng cho OutfitDetailDto.Attributes
            CreateMap<OutfitAttribute, OutfitAttributeInfo>().ReverseMap();

            // ReviewDTOs -> Model.Review
            CreateMap<Review, ReviewResponseDto>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ReviewImages))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));
            CreateMap<CreateReviewDto, Review>();
            CreateMap<UpdateReviewDto, Review>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ReviewImageDTOs -> Model.ReviewImage
            CreateMap<ReviewImage, ReviewImageResponseDto>().ReverseMap();
            CreateMap<CreateReviewImageDto, ReviewImage>();
            CreateMap<UpdateReviewImageDto, ReviewImage>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // OutfitDTOs -> Model.Outfit
            CreateMap<Outfit, OutfitResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
                .ForMember(dest => dest.TotalImages, opt => opt.MapFrom(src => src.OutfitImages != null ? src.OutfitImages.Count : 0))
                .ForMember(dest => dest.TotalSizes, opt => opt.MapFrom(src => src.OutfitSizes != null ? src.OutfitSizes.Count : 0))
                .ForMember(dest => dest.AvailableSizes, opt => opt.MapFrom(src =>
                    src.OutfitSizes != null ? src.OutfitSizes.Count(s => s.StockQuantity > 0) : 0))
                .ForMember(dest => dest.PrimaryImageUrl, opt => opt.MapFrom(src =>
                    src.OutfitImages != null && src.OutfitImages.Any()
                        ? src.OutfitImages.OrderBy(img => img.SortOrder).FirstOrDefault()!.ImageUrl
                        : null))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Reviews != null && src.Reviews.Any()
                        ? src.Reviews.Average(r => (double?)r.Rating)
                        : null))
                .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0));

            CreateMap<Outfit, OutfitDetailDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.OutfitImages))
                .ForMember(dest => dest.Sizes, opt => opt.MapFrom(src => src.OutfitSizes))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Reviews != null && src.Reviews.Any()
                        ? src.Reviews.Average(r => (double?)r.Rating)
                        : null))
                .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0))
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.OutfitAttribute));

            CreateMap<OutfitImage, OutfitImageInfo>();
            CreateMap<OutfitSize, OutfitSizeInfo>();

            CreateMap<CreateOutfitDto, Outfit>();
            CreateMap<UpdateOutfitDto, Outfit>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ServiceBookingDTOs -> Model.ServiceBooking
            CreateMap<ServiceBooking, ServiceBookingResponseDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.ServicePackageName, opt => opt.MapFrom(src => src.ServicePkg != null ? src.ServicePkg.Name : null))
                .ForMember(dest => dest.StudioName, opt => opt.MapFrom(src => src.ServicePkg != null && src.ServicePkg.Studio != null ? src.ServicePkg.Studio.Name : null))
                .ForMember(dest => dest.TotalAddons, opt => opt.MapFrom(src => src.ServiceBookingAddons != null ? src.ServiceBookingAddons.Count : 0));

            CreateMap<ServiceBooking, ServiceBookingDetailDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.UserPhoneNumber, opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : null))
                .ForMember(dest => dest.ServicePackageName, opt => opt.MapFrom(src => src.ServicePkg != null ? src.ServicePkg.Name : null))
                .ForMember(dest => dest.ServicePackageDescription, opt => opt.MapFrom(src => src.ServicePkg != null ? src.ServicePkg.Description : null))
                .ForMember(dest => dest.ServicePackageBasePrice, opt => opt.MapFrom(src => src.ServicePkg != null ? (decimal?)src.ServicePkg.BasePrice : null))
                .ForMember(dest => dest.StudioName, opt => opt.MapFrom(src => src.ServicePkg != null && src.ServicePkg.Studio != null ? src.ServicePkg.Studio.Name : null))
                .ForMember(dest => dest.StudioAddress, opt => opt.MapFrom(src => src.ServicePkg != null && src.ServicePkg.Studio != null ? src.ServicePkg.Studio.Address : null))
                .ForMember(dest => dest.StudioContactInfo, opt => opt.MapFrom(src => src.ServicePkg != null && src.ServicePkg.Studio != null ? src.ServicePkg.Studio.ContactInfo : null))
                .ForMember(dest => dest.Addons, opt => opt.MapFrom(src => src.ServiceBookingAddons));

            CreateMap<ServiceBookingAddon, ServiceBookingAddonInfo>()
                .ForMember(dest => dest.AddonName, opt => opt.MapFrom(src => src.Addon != null ? src.Addon.Name : null));

            CreateMap<CreateServiceBookingDto, ServiceBooking>();
            CreateMap<UpdateServiceBookingDto, ServiceBooking>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ServiceBooking, ServiceBookingDto>().ReverseMap();
            CreateMap<ServiceBookingAddon, ServiceBookingAddonDto>().ReverseMap();


            // Legacy DTO (keep for backward compatibility if needed)
            CreateMap<OutfitSize, OutfitSizeDTO>().ReverseMap();

            // RentalPackageDTOs -> Model.RentalPackage
            CreateMap<RentalPackage, RentalPackageDto>().ReverseMap();
            CreateMap<RentalPackage, RentalPackageDetailDto>().ReverseMap();
            CreateMap<RentalPackage, RentalPackageSelectDto>().ReverseMap();
            CreateMap<CreateRentalPackageDto, RentalPackage>().ReverseMap();
            CreateMap<UpdateRentalPackageDto, RentalPackage>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // =========================
            // RENTAL PACKAGE
            // =========================
            CreateMap<RentalPackage, RentalPackageDto>().ReverseMap();
            CreateMap<RentalPackage, RentalPackageDetailDto>().ReverseMap();
            CreateMap<RentalPackage, RentalPackageSelectDto>().ReverseMap();

            CreateMap<CreateRentalPackageDto, RentalPackage>();
            CreateMap<UpdateRentalPackageDto, RentalPackage>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // =========================
            // ADDRESS
            // ========================= 
            CreateMap<UserAddress, AddressDto>().ReverseMap();

            CreateMap<CreateAddressDto, UserAddress>()
                .ForMember(d => d.AddressId, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore()).ReverseMap();

            CreateMap<UpdateAddressDto, UserAddress>()
                .ForMember(d => d.AddressId, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore()).ReverseMap();

            // BookingDTOs -> Model.Booking
            CreateMap<Booking, BookingDto>().ReverseMap();
            CreateMap<BookingDetail, BookingDetailDto>().ReverseMap();

            // =========================
            // VNPAY / ORDER INFO MAPPING
            // =========================

            // BookingDTO -> OrderInfoDTo
            CreateMap<Booking, OrderInfoDTO>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.BookingId))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.BookingDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.OrderDesc, opt => opt.MapFrom(src => $"Transaction for booking:{src.BookingId}"))
                .ForMember(dest => dest.Amount, opt => opt.Ignore()); 

            // PaymentDTO -> OrderInfoDTO
            CreateMap<Payment, OrderInfoDTO>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => (long)(src.Amount * 100))) 
                .ForMember(dest => dest.PaymentTranId, opt => opt.MapFrom(src => src.TransactionRef))
                .ForMember(dest => dest.PayStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.BankCode, opt => opt.Ignore());

            // CategoryDTOs -> Model.Category
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<Category, UpdateCategoryDto>().ReverseMap();

            // StudioDTOs -> Model.Studio
            CreateMap<Studio, StudioDto>().ReverseMap();
            CreateMap<Studio, CreateStudioDto>().ReverseMap();
            CreateMap<Studio, UpdateStudioDto>().ReverseMap();

            // LoyaltyTransactionDto -> Model.LoyaltyTransaction
            CreateMap<LoyaltyTransaction, LoyaltyTransactionDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));
            CreateMap<CreateLoyaltyTransactionDto, LoyaltyTransaction>().ReverseMap();
            CreateMap<UpdateLoyaltyTransactionDto, LoyaltyTransaction>().ReverseMap();

        }
    }
}
