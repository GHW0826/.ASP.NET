using FluentValidation;
using GrpcStreaming;
using GrpcStreaming.Auth;
using GrpcStreaming.Context;
using GrpcStreaming.Repositories;
using GrpcStreaming.Repository;
using GrpcStreaming.Services;
using GrpcStreaming.Validators;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using GrpcStreaming.Interceptors;
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
            PermitLimit = 100, // 10회 허용
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});
builder.Services.AddScoped<GrpcStreaming.Services.UserService>(); // DI


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
app.MapGrpcService<GrpcStreaming.Services.ChatService>().RequireRateLimiting("global");
app.MapGrpcService<GrpcStreaming.Services.UserService>().RequireRateLimiting("global");  // UserService 등록
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
health.SetStatus("GrpcStreaming.UserService", HealthCheckResponse.Types.ServingStatus.Serving); // 특정 서비스명 (proto 패키지.서비스명)


app.Run();

/*

    gRPC의 3가지 스트리밍 패턴
 
    유형	                    설명
    Server Streaming	        서버가 클라이언트에게 여러 응답을 순차 전송
    Client Streaming	        클라이언트가 서버로 여러 요청을 스트리밍
    Bi-Directional Streaming	양방향 동시에 스트리밍 (실시간 통신 구조)

    gRPC는 HTTP/2 기반이라 Streaming을 Request-Response 기반에 비동기 스트림으로 처리 가능..
    .NET에서는 IAsyncStreamReader<T>, IServerStreamWriter<T> 등의 비동기 열거자 (async stream) 인터페이스로 처리.

    스트리밍 타입 요약
    타입	            방향	            설명
    Server Streaming	Server → Client	서버가 응답을 여러 개 전달 (ex. 실시간 주가, 뉴스 피드)
    Client Streaming	Client → Server	클라이언트가 여러 요청을 보낸 뒤 한번에 처리 (ex. 파일 업로드, 로그 수집)
    Bi-Directional	    Client ⇄ Server	    양방향으로 동시에 스트리밍 (ex. 채팅, 게임 패킷)


    [내부 구조와 흐름 이해]
    gRPC 내부에는 각 요청/응답마다 프레임 단위로 송수신되는 스트림 버퍼가 존재하며,
    .NET에서는 이를 IAsyncStreamReader<T> / IServerStreamWriter<T> 로 비동기 반복문 (await foreach) 형태로 사용.

    서버/클라이언트 모두 내부적으로 읽기 버퍼를 가짐
    실제로는 HTTP/2 프레임을 처리하는 비동기 큐
    데이터를 쓸 땐 await WriteAsync(...)
    데이터를 받을 땐 await foreach (... in ReadAllAsync())


    [핵심 포인트 요약]
    항목	            설명
    WriteAsync	        데이터를 보낼 때 사용 (비동기 큐에 쓰기)
    ReadAllAsync	    비동기적으로 받는 데이터 반복 (큐에서 읽기)
    내부 큐	            gRPC 내부에 HTTP/2 스트림 버퍼가 있음
    CompleteAsync	    클라이언트 스트림 종료 신호
    병렬 실행	        요청/응답 동시에 하려면 Task.Run 으로 분리
*/