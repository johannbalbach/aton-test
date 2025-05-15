using AutoMapper;
using TestApplication.DataAccessLayer.Dto;
using TestApplication.Domain.Entities;

namespace TestApplication.MappingProfile
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserCreateDto, User>();
            CreateMap<UserUpdateDto, User>();
            CreateMap<User, UserReadDto>()
                .ForMember(dest => dest.isRevoked, opt => opt.MapFrom(src => src.RevokedOn.HasValue));
        }
    }
}
