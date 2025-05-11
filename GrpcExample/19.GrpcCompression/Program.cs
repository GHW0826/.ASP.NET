using FluentValidation;
using GrpcCompression;
using GrpcCompression.Auth;
using GrpcCompression.Context;
using GrpcCompression.Repositories;
using GrpcCompression.Repository;
using GrpcCompression.Services;
using GrpcCompression.Validators;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using GrpcCompression.Interceptors;
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

        // Compression 설정
        options.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Fastest;
        options.ResponseCompressionAlgorithm = "gzip";
        options.EnableDetailedErrors = true;
        // Compression 설정
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
            PermitLimit = 100, // 10회 허용
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});
builder.Services.AddScoped<GrpcCompression.Services.UserService>(); // DI


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
app.MapGrpcService<GrpcCompression.Services.GreeterService>().RequireRateLimiting("global");
app.MapGrpcService<GrpcCompression.Services.FileService>().RequireRateLimiting("global");
app.MapGrpcService<GrpcCompression.Services.BIChatService>().RequireRateLimiting("global");
app.MapGrpcService<GrpcCompression.Services.ChatService>().RequireRateLimiting("global");
app.MapGrpcService<GrpcCompression.Services.UserService>().RequireRateLimiting("global");  // UserService 등록
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
health.SetStatus("GrpcCompression.UserService", HealthCheckResponse.Types.ServingStatus.Serving); // 특정 서비스명 (proto 패키지.서비스명)


app.Run();

/*
    gRPC 요청 및 응답에 gzip 압축을 적용하여,
    네트워크 전송량을 줄이고 대용량 데이터 응답 처리 성능을 향상


    [압축이 적용되는 구조]
    대상	                      설명
    클라이언트 요청	    CallOptions에 WriteOptions 설정 (CompressionLevel.Gzip)
    서버 응답	                서비스 또는 메시지 단위로 압축 여부 설정 가능



    비교	                비압축	      압축(gzip)
    전송 바이트	    높음	            낮음
    처리 속도	          느림	            빠름 (전송 시간 단축)
    CPU 부하	          낮음	            약간 증가

    ResponseCompressionAlgorithm은 서버 기본 응답 압축
    클라이언트는 WriteFlags.UseCompression으로 전송 요청 압축
    gRPC는 gzip만 기본 지원
*/