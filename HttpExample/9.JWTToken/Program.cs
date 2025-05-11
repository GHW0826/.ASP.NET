using JWTToken.Data;
using JWTToken.Middlewares;
using JWTToken.Validators;
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
 사용자가 로그인하면 JWT 토큰을 발급하고,
이 토큰을 Authorization 헤더에 포함해서 보호된 API에 접근하도록 함.

구현 구조
구분	                                설명
/api/auth/login	                    로그인 엔드포인트 → 토큰 발급
Authorization: Bearer {token}	  토큰을 포함한 요청 헤더
[Authorize]	                          보호된 API에 인증 필터 적용



Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.IdentityModel.Tokens;
System.IdentityModel.Tokens.Jwt;

JWT 설정 등록 (appsettings.json)
"Jwt": {
  "Key": "THIS_IS_A_SECRET_KEY_FOR_DEMO",   // 매우 중요: 프로덕션에선 환경변수 사용 권장
  "Issuer": "MyApiServer",
  "Audience": "MyApiClient",
  "ExpireMinutes": 60
}

 */