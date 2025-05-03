using AutoMapper;
using HealthCheckEndpoint.Models;
using HealthCheckEndpoint.Models.Dto;

namespace HealthCheckEndpoint.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}