using SignupWithdrawal.Models;
using SignupWithdrawal.Models.Dtos;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SignupWithdrawal.Profiles
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
