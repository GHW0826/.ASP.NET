using AutoMapper;
using GrpcGateway.Models;
using GrpcGateway.Models.Dto;

namespace GrpcGateway.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}