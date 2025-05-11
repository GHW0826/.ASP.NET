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


builder.Services.AddControllers(); // REST API용
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=users.db")); // SQLite

builder.Services.AddSingleton<JwtProvider>();
builder.Services.AddGrpc(options =>
    {
        options.Interceptors.Add<JwtInterceptor>(); // gRPC 인터셉터 등록
    });
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper 등록
// builder.Services.AddSingleton<UserRepository>();  // DI 등록
builder.Services.AddGrpc();

var app = builder.Build();

app.MapControllers();
// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<JWTAuth.Services.UserService>();  // UserService 등록
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

/*
    JWT 발급용 Login API 구현 (REST 방식)
    gRPC 클라이언트가 Metadata에 JWT 포함
    gRPC 서버에서 Interceptor로 토큰 검증
    사용자 인증된 경우만 gRPC API 접근 허용
*/