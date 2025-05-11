using AutoMapper;
using GrpcCompression.Models;
using GrpcCompression.Models.Dto;

namespace GrpcCompression.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}