using AutoMapper;
using Common.Entities;
using Common.DTOs;

namespace Services.Mappers {
    public class UserMappingProfile : Profile {
        public UserMappingProfile() {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.SecondName));

            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.SecondName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<UserCreateUpdateDTO, User>()
                .ForMember(dest => dest.SecondName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.RoleName, opt => opt.Ignore());
        }
    }
}
