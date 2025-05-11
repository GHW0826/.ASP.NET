using FluentValidation;
using PrometheusMetrics;
using PrometheusMetrics.Auth;
using PrometheusMetrics.Context;
using PrometheusMetrics.Repositories;
using PrometheusMetrics.Repository;
using PrometheusMetrics.Services;
using PrometheusMetrics.Validators;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using PrometheusMetrics.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(); // REST API용
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=users.db")); // SQLite

builder.Services.AddSingleton<JwtProvider>();
builder.Services.AddGrpc(options =>
    {
        options.Interceptors.Add<JwtInterceptor>();         // gRPC 인터셉터 등록
        options.Interceptors.Add<RoleInterceptor>();        // 역할 인가 처리 추가
        options.Interceptors.Add<ExceptionInterceptor>();  // 예외 처리
        options.Interceptors.Add<GrpcMetricsInterceptor>();
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

// Prometheus /metrics 엔드포인트 등록
app.UseRouting();
// 반드시 UseHttpMetrics()를 먼저 등록
app.UseHttpMetrics(); // 기본 HTTP/REST 메트릭 수집
app.UseAuthentication();
app.UseAuthorization();
app.MapMetrics("/metrics");

app.MapControllers();
// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<PrometheusMetrics.Services.UserService>();  // UserService 등록
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            results = report.Entries.Select(e => new {
                key = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });

        await context.Response.WriteAsync(result);
    }
});

app.Run();

/*
    ASP.NET Core 앱에 Prometheus 수집 가능한 /metrics 엔드포인트를 추가하여
    gRPC 및 REST 요청 처리 현황, 응답 시간 등을 실시간 모니터링할 수 있도록 구성.

    prometheus-net.AspNetCore	    ASP.NET Core용 Prometheus 미들웨어
    prometheus-net	                내부 Counter, Histogram API 제공


    [Prometheus 서버 연결]
    prometheus.yml 예:

    scrape_configs:
      - job_name: 'grpc_app'
        metrics_path: /metrics
        static_configs:
          - targets: ['localhost:7007']

    
*/