using Dockerfiles.Models;
using Dockerfiles.Models.Dtos;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dockerfiles.Profiles
{
    public class TodoProfile : Profile
    {
        public TodoProfile()
        {
            CreateMap<TodoItem, GetTodoDto>();

            CreateMap<CreateTodoDto, TodoItem>();
            CreateMap<UpdateTodoDto, TodoItem>();
        }
    }
}
