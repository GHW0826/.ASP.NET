using FluentValidation;
using GrpcHealthcheck;
using GrpcHealthcheck.Auth;
using GrpcHealthcheck.Context;
using GrpcHealthcheck.Repositories;
using GrpcHealthcheck.Repository;
using GrpcHealthcheck.Services;
using GrpcHealthcheck.Validators;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using GrpcHealthcheck.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Prometheus;
using System.Threading.RateLimiting;
using Grpc.HealthCheck;
using Grpc.Health.V1;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(); // REST API용
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<HealthServiceImpl>();
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

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("global", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString();

        // 로그 출력
        Console.WriteLine($">> RateLimit 요청 IP: {ip}");
        var key = ip ?? "unknown";
        // 키도 출력 (확인용)
        Console.WriteLine($">> Partition Key: {key}");

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10, // 10회 허용
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});
builder.Services.AddScoped<GrpcHealthcheck.Services.UserService>(); // DI


var app = builder.Build();

app.UseRouting();       
app.UseRateLimiter(); // 순서 중요! Authentication보다 위로도 가능
// Prometheus /metrics 엔드포인트 등록
// 반드시 UseHttpMetrics()를 먼저 등록
app.UseHttpMetrics(); // 기본 HTTP/REST 메트릭 수집
app.UseAuthentication();
app.UseAuthorization();
app.MapMetrics("/metrics");


app.MapControllers().RequireRateLimiting("global"); ;
// Configure the HTTP request pipeline.
// 등록
app.MapGrpcService<HealthServiceImpl>();
app.MapGrpcService<GreeterService>().RequireRateLimiting("global");
app.MapGrpcService<GrpcHealthcheck.Services.UserService>().RequireRateLimiting("global");  // UserService 등록
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

var health = app.Services.GetRequiredService<HealthServiceImpl>();
health.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving); // 전체 서비스
health.SetStatus("GrpcHealthcheck.UserService", HealthCheckResponse.Types.ServingStatus.Serving); // 특정 서비스명 (proto 패키지.서비스명)


app.Run();

/*
    gRPC 전용 Health 서비스를 통해 서버의 상태를 gRPC 클라이언트가 확인할 수 있도록 구성.
    gRPC 표준에 맞는 grpc.health.v1.Health 서비스 사용
    REST /health 와는 별도
    클라이언트에서 HealthClient.CheckAsync() 호출 가능


    // client
    try
    {
        var response = await client.CheckAsync(new HealthCheckRequest { Service = "" }); // 전체
        Console.WriteLine($"gRPC Health 상태: {response.Status}");
    }
    catch (RpcException ex)
    {
        Console.WriteLine($"gRPC Health 오류: {ex.Status.Detail}");
    }
*/