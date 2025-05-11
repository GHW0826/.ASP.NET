using RefreshTokens.Data;
using RefreshTokens.Middlewares;
using RefreshTokens.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// FluentValidation 설정
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<TodoCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<TodoUpdateDtoValidator>();
// FluentValidation 설정


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program));

// DbContext 등록 (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
// DbContext 등록 (SQLite)



// JWT 설정 바인딩
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
// JWT 설정 바인딩


// API Versioning 등록
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0); // 기본값 v1.0
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // v1, v2 식 이름
    options.SubstituteApiVersionInUrl = true;
});
// API Versioning 등록

// Swagger 설정
builder.Services.AddSwaggerGen(options =>
{
    // API Versioning 등록
    var provider = builder.Services.BuildServiceProvider()
                    .GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var desc in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(desc.GroupName, new OpenApiInfo
        {
            Title = $"Todo API {desc.ApiVersion}",
            Version = desc.GroupName
        });
    }
    // API Versioning 등록

    // JWT 인증을 위한 Security 정의 추가 Security Requirement 등록
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT 인증 토큰을 입력하세요. 예: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    // JWT 인증을 위한 Security 정의 추가 Security Requirement 등록

});
// Swagger 설정



var app = builder.Build();

// Middleware 설정
app.UseMiddleware<ExceptionMiddleware>();
// Middleware 설정


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
            // API Versioning 등록
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
        }
            // API Versioning 등록
    });
}

app.UseHttpsRedirection();

// JWT 설정 반드시 UseAuthorization() 전
app.UseAuthentication();
// JWT 설정

app.UseAuthorization();

app.MapControllers();

app.Run();



/*
    액세스 토큰(JWT)은 짧게, Refresh Token은 길게 유지
    액세스 토큰이 만료되면 Refresh Token으로 재로그인 없이 재발급 가능

    [Refresh Token 구조 요약]
    구성	            설명
    AccessToken	        기존 JWT, 수명 짧음 (예: 10~30분)
    RefreshToken	    DB에 저장된 토큰, 수명 김 (예: 7~14일)
    /auth/login	        로그인 시 두 토큰 모두 발급
    /auth/refresh	    AccessToken 만료 시 RefreshToken으로 재발급 요청


    [사용 흐름]
    1. POST /api/auth/login
        → accessToken + refreshToken 받음

    2. accessToken 만료됨 (예: 30분 후)

    3. POST /api/auth/refresh 로 refreshToken 전송
        → 새로운 accessToken + refreshToken 재발급
 */