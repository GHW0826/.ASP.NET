using AutoMapper;
using RateLimitings.Models;
using RateLimitings.Models.Dto;

namespace RateLimitings.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}