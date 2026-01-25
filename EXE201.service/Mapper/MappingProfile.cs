using AutoMapper;
using EXE201.Repository.Models;
using EXE201.Service.DTOs;
using EXE201.Service.DTOs.UserDTOs;
using EXE201.Service.DTOs.WishlistDTOs;
using EXE201.Service.DTOs.OutfitImageDTOs;
using EXE201.Service.DTOs.OutfitSizeDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // UserDTOs -> Model.User
            CreateMap<User, ListUserDto>().ReverseMap();
            CreateMap<User, UserDetailDto>().ReverseMap();
            CreateMap<UpdateUserProfileDto, User>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<User,UserDTO>().ForMember(opt => opt.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));
    
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
      
            // Legacy DTO (keep for backward compatibility if needed)
            CreateMap<OutfitSize, OutfitSizeDTO>().ReverseMap();
        }
    }
}
