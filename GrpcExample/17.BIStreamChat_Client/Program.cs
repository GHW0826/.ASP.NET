using BIStreamingChat;
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

var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new BIChatService.BIChatServiceClient(channel);
using var call = client.Chat();
var receive = Task.Run(async () =>
{
    await foreach (var msg in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"[Server -> {msg.User}] {msg.Message}");
    }
});

var userName = args.Length > 0 ? args[0] : $"User{Guid.NewGuid().ToString()[..4]}";

string name = userName;
for (int i = 0; i < 5; ++i)
{
    await call.RequestStream.WriteAsync(new BIChatMessage
    {
        User = name,
        Message = $"Hello {i}",
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    });
    await Task.Delay(1000);
}

await call.RequestStream.CompleteAsync();
await receive;


/*
*/