using Dockerfiles.Data;
using Dockerfiles.Middlewares;
using Dockerfiles.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Dockerfiles.Models;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Dockerfiles.Repositories;
using Dockerfiles.Services;
using Dockerfiles.Filters;

var AllowSpecificOrigins = "_myAllowSpecificOrigins";


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Filter 등록
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
// Filter 등록


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

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")  // 허용할 클라이언트 Origin
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // JWT 쿠키 인증 시 필요
        });
});



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
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                // 기본 401 HTML 제거
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var response = JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = "로그인이 필요합니다.",
                    Code = "Unauthorized"
                });
                return context.Response.WriteAsync(response);
                /*
                var errorJson = JsonSerializer.Serialize(ApiResponse<string>.Fail("로그인이 필요합니다."));
                await context.Response.WriteAsync(errorJson);
                */
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                var response = JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = "권한이 없습니다.",
                    Code = "Forbidden"
                });
                return context.Response.WriteAsync(response);
            }
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


// HealthCheck 설정
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"), name: "sql", tags: new[] { "db" });
// HealthCheck 설정


// Layer 설정
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();
// Layer 설정


var app = builder.Build();

app.UseCors(AllowSpecificOrigins);
// Middleware 설정
app.UseMiddleware<ExceptionMiddleware>();
// Middleware 설정

// HealthCheck 설정
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
// HealthCheck 설정


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

Controllers/
  └── TodoController.cs      ← 요청 처리 (입출력만 담당)

Services/
  └── ITodoService.cs        ← 비즈니스 로직 인터페이스
  └── TodoService.cs         ← 실제 비즈니스 로직 구현

Repositories/
  └── ITodoRepository.cs     ← 데이터 접근 인터페이스
  └── TodoRepository.cs      ← 실제 DB 쿼리 구현


 */