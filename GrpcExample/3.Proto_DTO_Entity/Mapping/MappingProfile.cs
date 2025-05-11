using AutoMapper;
using Proto_DTO_Entity.Models;
using Proto_DTO_Entity.Models.Dto;

namespace Proto_DTO_Entity.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}