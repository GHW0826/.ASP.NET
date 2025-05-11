using DBAuth.Data;
using DBAuth.Middlewares;
using DBAuth.Validators;
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
    Users 테이블을 만들어 사용자 정보를 저장하고,
    로그인 시 DB에서 사용자 조회 + 비밀번호 확인 → JWT 발급


    User 모델	            사용자 ID, 이름, 비밀번호 필드 포함
    AppDbContext.Users	    EF Core를 통해 DB 연동
    /api/auth/login	        DB에서 사용자 조회 + 비번 검증 → 토큰 발급

 */