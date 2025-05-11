using AutoMapper;
using GrpcDeadlineTimeout.Models;
using GrpcDeadlineTimeout.Models.Dto;

namespace GrpcDeadlineTimeout.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}