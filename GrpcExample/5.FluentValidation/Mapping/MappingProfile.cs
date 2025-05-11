using AutoMapper;
using FluentValidations.Models;
using FluentValidations.Models.Dto;

namespace FluentValidations.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}