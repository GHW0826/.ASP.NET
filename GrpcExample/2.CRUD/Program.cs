using CRUD;
using CRUD.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<UserRepository>();  // DI 등록
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<CRUD.Services.UserService>();  // UserService 등록
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

/*
    gRPC 기반 Create / Get / Update / Delete 메서드 정의
    In-memory 저장소로 CRUD 로직 구현
    클라이언트에서 gRPC 호출로 사용자 관리
*/