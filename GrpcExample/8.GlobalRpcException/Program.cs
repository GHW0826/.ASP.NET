using FluentValidation;
using GlobalRpcException;
using GlobalRpcException.Auth;
using GlobalRpcException.Context;
using GlobalRpcException.Repositories;
using GlobalRpcException.Repository;
using GlobalRpcException.Services;
using GlobalRpcException.Validators;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using GlobalRpcException.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(); // REST API용
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=users.db")); // SQLite

builder.Services.AddSingleton<JwtProvider>();
builder.Services.AddGrpc(options =>
    {
        options.Interceptors.Add<JwtInterceptor>();         // gRPC 인터셉터 등록
        options.Interceptors.Add<RoleInterceptor>();        // 역할 인가 처리 추가
        options.Interceptors.Add<ExceptionInterceptor>();  // 예외 처리
    });
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper 등록

var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection["key"];
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            RoleClaimType = ClaimTypes.Role
        };
    });
var app = builder.Build();

app.MapControllers();
// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<GlobalRpcException.Services.UserService>();  // UserService 등록
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

/*
    공통 Interceptor에서 예외를 잡고 → RpcException으로 변환
    개발 중엔 로그 출력도 함께 지원

    예상치 못한 예외도 공통 포맷으로 처리됨
    로그 출력은 개발 시 디버깅에 유용함
*/