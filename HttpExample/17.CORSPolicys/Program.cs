using CORSPolicys.Data;
using CORSPolicys.Middlewares;
using CORSPolicys.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CORSPolicys.Models;
using System.Text.Json;

var AllowSpecificOrigins = "_myAllowSpecificOrigins";


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



var app = builder.Build();

app.UseCors(AllowSpecificOrigins);
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

    CORS는 브라우저 보안 정책이다.
    다른 도메인(Origin)에서 스크립트로 API를 호출할 때 브라우저가 보안상 막는 걸 풀어주는 규칙.
    
    [Origin이란]
    Origin = {프로토콜} + {도메인} + {포트} 조합

    요청	                        동일 Origin?
    API: http://localhost:5000
    클라: http://localhost:3000	    (포트 다름)

    API: https://myapi.com
    클라: https://myapi.com	        (완전 동일)    

    
    웹 브라우저는 보안을 위해 다른 Origin에서 JS가 호출하는 API는 기본적으로 차단

    // http://localhost:3000 에서 실행되는 JS
    fetch("http://localhost:5000/api/todo") 
    // → 브라우저가 CORS preflight 검사 → 허용 안 되면 차단
    
    브라우저는 악성 JS 코드가 다른 도메인의 쿠키나 데이터에 무단 접근하는 걸 방지해야 하니까
    → 기본적으로 다른 Origin 요청은 차단


    [정리]
    항목	        설명
    누가 막나?	    서버가 아님 → 브라우저가 차단
    왜 막나?	    내 쿠키, 세션, 저장된 인증 정보가 내가 의도하지 않은 사이트로 나가지 않게 하기 위해
    서버는?	        단지 "Access-Control-Allow-Origin" 헤더로 허용 여부만 응답
    Postman은?	    쿠키가 없으니 그런 보안 걱정 필요 없음 → CORS 무시


    [흐름 정리]
    [1] 사용자가 shopping.com 접속
         ↓
    [2] JS 코드가 bank.com에 fetch 요청 보냄
         ↓
    [3] 브라우저: "어? shopping.com → bank.com? 이건 cross-origin인데?"
         ↓
    [4] 브라우저: OPTIONS 요청을 bank.com에 먼저 보냄 (Preflight 요청)
         ↓
    [5] bank.com이 Access-Control-Allow-Origin: shopping.com 으로 응답
         ↓
    [6] 브라우저: "좋아, bank.com이 허용했어" → 진짜 GET/POST 요청 보냄




    기능	                        설명
    CORS 허용	                    지정한 도메인(예: http://localhost:3000)에서만 API 호출 허용
    사전 요청(OPTIONS) 허용	        브라우저의 preflight 요청 정상 처리
    개발/운영 환경 분리 가능	    환경별로 Origin 설정 가능


    [옵션 요약]
    메서드	                설명
    WithOrigins(...)	    허용할 프론트엔드 도메인 (정확한 Origin만 허용)
    AllowAnyHeader()	    모든 헤더 허용 (예: Authorization)
    AllowAnyMethod()	    GET, POST, PUT, DELETE 등 허용
    AllowCredentials()	    인증 정보 포함 요청 허용 (예: 쿠키, 인증 헤더)
    
    개발용으로는 "http://localhost:3000"
    운영용은 "https://myfrontend.com" 이런 식으로 설정 가능

    
    [테스트 방법]
    1. 프론트엔드에서 다른 포트나 Origin에서 호출 시:
        허용된 Origin이면 정상 호출됨
        미허용 Origin이면 브라우저에서 CORS 차단 오류 발생
    2. Swagger 테스트는 localhost에서 도는 경우 별 문제 없음


 */