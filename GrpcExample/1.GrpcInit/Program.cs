using GrpcInit.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<HelloService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


/*
    .proto 파일을 작성하여 gRPC 서비스 정의
    서버에 gRPC 서비스 구현 (SayHello)
    클라이언트에서 gRPC 호출 (Grpc.Net.Client 사용) 
*/