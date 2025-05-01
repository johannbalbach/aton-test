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
            CreateMap<User, UserReadDto>();
            CreateMap<UserUpdateDto, User>();
        }
    }
}
