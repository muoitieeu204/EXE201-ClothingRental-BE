using AutoMapper;
using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using EXE201.Service.DTOs.OutfitAttributeDTOs;
using EXE201.Service.DTOs.OutfitDTOs;
using EXE201.Service.DTOs.OutfitImageDTOs;
using EXE201.Service.DTOs.OutfitSizeDTOs;
using EXE201.Service.DTOs.ReviewDTOs;
using EXE201.Service.DTOs.ReviewImageDTOs;
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

            // Legacy DTO (keep for backward compatibility if needed)
            CreateMap<OutfitSize, OutfitSizeDTO>().ReverseMap();
        }
    }
}
