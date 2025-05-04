using AutoMapper;
using GrpcHealthcheck.Models;
using GrpcHealthcheck.Models.Dto;

namespace GrpcHealthcheck.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}