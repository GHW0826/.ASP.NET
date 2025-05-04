using AutoMapper;
using FileUploadDownload.Models;
using FileUploadDownload.Models.Dto;

namespace FileUploadDownload.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserDto, UserResponse>().ReverseMap();
    }
}