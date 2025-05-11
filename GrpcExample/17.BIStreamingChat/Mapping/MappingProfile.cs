using AutoMapper;
using BIStreamingChat.Models;
using BIStreamingChat.Models.Dto;

namespace BIStreamingChat.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}