using Proto_DTO_Entity;
using Proto_DTO_Entity.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper ���
builder.Services.AddSingleton<UserRepository>();  // DI ���
// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<Proto_DTO_Entity.Services.UserService>();  // UserService ���
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


/*
    DB Entity �� DTO �� gRPC �޽���(Proto) ������ å���� �и�
    AutoMapper�� ��ȯ �ڵ�ȭ
    ���� ���ο� ����Ͻ� ���� �߽� DTO ����
*/
/*
    ���� ���	        ����
    User	            DB �� (Entity)
    UserDto	            ���� ����/������ ��
    UserResponse	    �ܺο� gRPC �޽���
    AutoMapper	        Entity �� DTO �� Proto ��ȯ �ڵ�ȭ
*/