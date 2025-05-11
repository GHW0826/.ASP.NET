using FluentValidation;
using JWTAuth;
using JWTAuth.Auth;
using JWTAuth.Context;
using JWTAuth.Repositories;
using JWTAuth.Repository;
using JWTAuth.Services;
using JWTAuth.Validators;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(); // REST API��
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=users.db")); // SQLite

builder.Services.AddSingleton<JwtProvider>();
builder.Services.AddGrpc(options =>
    {
        options.Interceptors.Add<JwtInterceptor>(); // gRPC ���ͼ��� ���
    });
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper ���
// builder.Services.AddSingleton<UserRepository>();  // DI ���
builder.Services.AddGrpc();

var app = builder.Build();

app.MapControllers();
// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<JWTAuth.Services.UserService>();  // UserService ���
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

/*
    JWT �߱޿� Login API ���� (REST ���)
    gRPC Ŭ���̾�Ʈ�� Metadata�� JWT ����
    gRPC �������� Interceptor�� ��ū ����
    ����� ������ ��츸 gRPC API ���� ���
*/