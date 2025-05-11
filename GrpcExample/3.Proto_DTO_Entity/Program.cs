using Proto_DTO_Entity;
using Proto_DTO_Entity.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper 등록
builder.Services.AddSingleton<UserRepository>();  // DI 등록
// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<Proto_DTO_Entity.Services.UserService>();  // UserService 등록
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


/*
    DB Entity ↔ DTO ↔ gRPC 메시지(Proto) 사이의 책임을 분리
    AutoMapper로 변환 자동화
    서비스 내부에 비즈니스 로직 중심 DTO 설계
*/
/*
    구성 요소	        설명
    User	            DB 모델 (Entity)
    UserDto	            내부 전송/가공용 모델
    UserResponse	    외부용 gRPC 메시지
    AutoMapper	        Entity ↔ DTO ↔ Proto 변환 자동화
*/