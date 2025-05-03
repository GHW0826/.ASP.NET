using Grpc.Net.Client;
using GrpcInit;
using Grpc.Core;
using GrpcClient;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using RoleAuth;
using Google.Protobuf.WellKnownTypes;

var address = "https://localhost:7007";
var endpoint = $"{address}/api/auth/login";
var healthendpoint = $"{address}/health";



// healthcheck
var req2 = new HttpRequestMessage(HttpMethod.Get, healthendpoint) { Version = new Version(2, 0) };
var response2 = await httpClient.SendAsync(req2).ConfigureAwait(false);
if (!response2.IsSuccessStatusCode)
{
    Console.WriteLine(" healthcheck 실패");
    return;
}
var healthcheckContent = await response2.Content.ReadAsStringAsync();
Console.WriteLine($" Health check 결과: {healthcheckContent}");


// TOKEN
var httpClient = new HttpClient();
var loginReq = new AuthUser { UserId = "alice", Role = "Admin" };
var json = JsonSerializer.Serialize(loginReq);
var content = new StringContent(json, Encoding.UTF8, "application/json");
var req1 = new HttpRequestMessage(HttpMethod.Post, endpoint) { Version = new Version(2, 0), Content = content };
var response1 = await httpClient.SendAsync(req1).ConfigureAwait(false);
if (!response1.IsSuccessStatusCode)
{
    Console.WriteLine("로그인 실패");
    return;
}
var loginRes = await response1.Content.ReadFromJsonAsync<LoginResponse>();
var token = loginRes!.token;
var headers = new Metadata
{
    { "authorization", $"Bearer {token}" }
};

var channel = GrpcChannel.ForAddress(address);
var client = new UserService.UserServiceClient(channel);

var adminRes = await client.AdminOnlyAsync(new Empty(), headers);
Console.WriteLine(adminRes.Message);


var create = await client.CreateUserAsync(new CreateUserRequest { Name = "Alice", Age = 25 }, headers);
Console.WriteLine($"[Create] Id={create.Id}, Name={create.Name}, Age={create.Age}");

var get = await client.GetUserAsync(new GetUserRequest { Id = create.Id }, headers);
Console.WriteLine($"[Get] Id={get.Id}, Name={get.Name}, Age={get.Age}");

var update = await client.UpdateUserAsync(new UpdateUserRequest { Id = create.Id, Name = "Alice Kim", Age = 26 }, headers);
Console.WriteLine($"[Update] Id={update.Id}, Name={update.Name}, Age={update.Age}");

var delete = await client.DeleteUserAsync(new DeleteUserRequest { Id = create.Id }, headers);
Console.WriteLine($"[Delete] Success={delete.Success}");

