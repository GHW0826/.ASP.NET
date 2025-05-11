using HttpHealthcheck.Models;
using HttpHealthcheck.Models.Dtos;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HttpHealthcheck.Profiles
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
