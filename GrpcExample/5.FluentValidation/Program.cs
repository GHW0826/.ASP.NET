using FluentValidation;
using FluentValidations;
using FluentValidations.Context;
using FluentValidations.Repositories;
using FluentValidations.Repository;
using FluentValidations.Services;
using FluentValidations.Validators;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=users.db")); // SQLite

builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper 등록
// builder.Services.AddSingleton<UserRepository>();  // DI 등록
// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FluentValidations.Services.UserService>();  // UserService 등록
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

/*
    CreateUserRequest, UpdateUserRequest 등의 필드에 유효성 검사 적용
    실패 시 RpcException으로 통일된 오류 전송
    Validator를 gRPC 서비스에서 호출하는 구조 설계
*/