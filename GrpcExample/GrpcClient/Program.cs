using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using GrpcClient;
using GrpcStreaming;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

var address = "https://localhost:7007";
var endpoint = $"{address}/api/auth/login";
var healthendpoint = $"{address}/health";


var httpClient = new HttpClient();

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

var callOptions = new CallOptions()
.WithWriteOptions(new WriteOptions(WriteFlags.UseCompression));
var adminRes = await client.AdminOnlyAsync(new Empty(), headers, callOptions);
Console.WriteLine(adminRes.Message);


var create = await client.CreateUserAsync(new CreateUserRequest { Name = "Alice", Age = 25 }, headers);
Console.WriteLine($"[Create] Id={create.Id}, Name={create.Name}, Age={create.Age}");


var gatewayendpoint = $"{address}/api/gateway/UserGateway/11";
var req3 = new HttpRequestMessage(HttpMethod.Get, gatewayendpoint) { Version = new Version(2, 0) };
req3.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
var response3 = await httpClient.SendAsync(req3).ConfigureAwait(false);
if (!response3.IsSuccessStatusCode)
{
    Console.WriteLine("request 실패");
    return;
}
var gatewayContent = await response3.Content.ReadAsStringAsync();
Console.WriteLine($" gatewayContent 결과: {gatewayContent}");


// Grpc.HealthCheck
var healthclient = new Health.HealthClient(channel);
try
{
    var response = await healthclient.CheckAsync(new HealthCheckRequest { Service = "" }, headers); // 전체
    Console.WriteLine($"gRPC Health 상태: {response.Status}");
}
catch (RpcException ex)
{
    Console.WriteLine($"gRPC Health 오류: {ex.Status.Detail}");
}


// ServerStreaming (서버가 여러 개 응답)
var chatClient = new ChatService.ChatServiceClient(channel);
using var call = chatClient.ServerStream(new Message { Sender = "Alice", Content = "Hi!" }, headers);
await foreach (var reply in call.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"[ServerStreaming Server -> Client] {reply.Content}");
}

// ClientStreaming (클라이언트가 여러 번 전송)
using var call2 = chatClient.ClientStream();
for (int i = 0; i < 5; i++)
{
    await call2.RequestStream.WriteAsync(new Message
    {
        Sender = "Alice",
        Content = $"Message {i}",
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    });
}
await call2.RequestStream.CompleteAsync(); // ❗ 중요: 완료 신호
var ack = await call2.ResponseAsync;
Console.WriteLine($"ClientStreaming 서버 응답: {ack.Status}");


// BiDirectional Streaming (양방향)
using var call3 = chatClient.BiStream();

// 응답 받기
var responseTask = Task.Run(async () =>
{
    await foreach (var response in call3.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"[BiDirectional Server] {response.Content}");
    }
});

// 요청 보내기
for (int i = 0; i < 2; i++)
{
    await call3.RequestStream.WriteAsync(new Message
    {
        Sender = "Alice",
        Content = $"Message {i}",
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    });
    await Task.Delay(500);
}

await call3.RequestStream.CompleteAsync(); // 요청 완료
await responseTask; // 응답 받기 완료


return;

try
{
    for (int i = 0; i < 20; ++i)
    {
        var get = await client.GetUserAsync(new GetUserRequest { Id = create.Id }, headers);
        Console.WriteLine($"[Get] Id={get.Id}, Name={get.Name}, Age={get.Age}");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"❌ HttpRequestException: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ 일반 예외: {ex.Message}");
}


var update = await client.UpdateUserAsync(new UpdateUserRequest { Id = create.Id, Name = "Alice Kim", Age = 26 }, headers);
Console.WriteLine($"[Update] Id={update.Id}, Name={update.Name}, Age={update.Age}");

var delete = await client.DeleteUserAsync(new DeleteUserRequest { Id = create.Id }, headers);
Console.WriteLine($"[Delete] Success={delete.Success}");

