using AutoMapper;
using PrometheusMetrics.Models;
using PrometheusMetrics.Models.Dto;

namespace PrometheusMetrics.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}