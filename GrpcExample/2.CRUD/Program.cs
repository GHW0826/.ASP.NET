using CRUD;
using CRUD.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<UserRepository>();  // DI ���
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<CRUD.Services.UserService>();  // UserService ���
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

/*
    gRPC ��� Create / Get / Update / Delete �޼��� ����
    In-memory ����ҷ� CRUD ���� ����
    Ŭ���̾�Ʈ���� gRPC ȣ��� ����� ����
*/