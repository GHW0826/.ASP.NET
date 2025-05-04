using AutoMapper;
using GrpcStreaming.Models;
using GrpcStreaming.Models.Dto;

namespace GrpcStreaming.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}