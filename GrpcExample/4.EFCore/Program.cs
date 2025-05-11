using EFCore.Context;
using EFCore.Repositories;
using EFCore.Repository;
using EFCore.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=users.db")); // SQLite

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper ���
// builder.Services.AddSingleton<UserRepository>();  // DI ���
// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<EFCore.Services.UserService>();  // UserService ���
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

/*
    User Entity�� SQLite DB�� ����
    EF Core DbContext ����
    ���� UserRepository�� EF ������� ��ȯ

    Microsoft.EntityFrameworkCore
    Microsoft.EntityFrameworkCore.Sqlite
    Microsoft.EntityFrameworkCore.Design
*/