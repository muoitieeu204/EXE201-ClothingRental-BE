using AutoMapper;
using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using EXE201.Service.DTOs.AddressDTOs;
using EXE201.Service.DTOs.BookingDTOs;
using EXE201.Service.DTOs.OutfitAttributeDTOs;
using EXE201.Service.DTOs.OutfitDTOs;
using EXE201.Service.DTOs.OutfitImageDTOs;
using EXE201.Service.DTOs.OutfitSizeDTOs;
using EXE201.Service.DTOs.RentalPackageDTOs;
using EXE201.Service.DTOs.ServiceAddonDTOs;
using EXE201.Service.DTOs.ServiceBookingDTOs;
using EXE201.Service.DTOs.ServicePackageDTOs;
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
            // SERVICE ADDON
            // =========================
            CreateMap<ServiceAddon, ServiceAddonDto>().ReverseMap();
            CreateMap<ServiceAddon, ServiceAddonSelectDto>();

            CreateMap<CreateServiceAddonDto, ServiceAddon>();
            CreateMap<UpdateServiceAddonDto, ServiceAddon>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // =========================
            // SERVICE PACKAGE
            // =========================
            CreateMap<ServicePackage, ServicePackageDto>().ReverseMap();
            CreateMap<ServicePackage, ServicePackageSelectDto>();

            CreateMap<ServicePackage, ServicePackageDetailDto>()
                .ForMember(d => d.Addons, opt => opt.MapFrom(s => s.ServiceAddons));

            // map ServiceAddon -> ServiceAddonInfoDto (for ServicePackageDetailDto)
            CreateMap<ServiceAddon, ServiceAddonInfoDto>();

            CreateMap<CreateServicePackageDto, ServicePackage>();
            CreateMap<UpdateServicePackageDto, ServicePackage>()
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

        }
    }
}
