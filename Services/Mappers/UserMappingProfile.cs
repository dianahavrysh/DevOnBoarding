using AutoMapper;
using Common.Entities;
using Common.DTOs;

namespace Services.Mappers {
    public class UserMappingProfile : Profile {
        public UserMappingProfile() {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.SecondName, opt => opt.MapFrom(src => src.SecondName));

            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.SecondName, opt => opt.MapFrom(src => src.SecondName));

            CreateMap<UserCreateUpdateDTO, User>()
                .ForMember(dest => dest.SecondName, opt => opt.MapFrom(src => src.SecondName))
                .ForMember(dest => dest.RoleName, opt => opt.Ignore());
        }
    }
}
