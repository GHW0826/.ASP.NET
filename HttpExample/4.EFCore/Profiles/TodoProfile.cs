using _4.EFCore.Models;
using _4.EFCore.Models.Dtos;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace _4.EFCore.Profiles
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
