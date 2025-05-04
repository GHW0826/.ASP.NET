using Grpc.Net.Client;
using Grpc.Core;
using GrpcClient;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using RoleAuth;
using Google.Protobuf.WellKnownTypes;
using Polly;


var policy = Policy
    .Handle<RpcException>(ex =>
        ex.StatusCode == StatusCode.Unavailable ||
        ex.StatusCode == StatusCode.DeadlineExceeded)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (exception, timespan, retryCount, context) =>
        {
            Console.WriteLine($"[{retryCount}] Retry after {timespan.TotalSeconds}s: {exception.Message}");
        });


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


var channel = GrpcChannel.ForAddress(address);
var client = new UserService.UserServiceClient(channel);


var adminRes = await client.AdminOnlyAsync(new Empty(), headers);
Console.WriteLine(adminRes.Message);


var create = await client.CreateUserAsync(new CreateUserRequest { Name = "Alice", Age = 25 }, headers);
Console.WriteLine($"[Create] Id={create.Id}, Name={create.Name}, Age={create.Age}");



// 클라이언트는 3회 재시도 후 실패 출력됨
try
{
    var result = await policy.ExecuteAsync(async () =>
    {
        return await client.GetUserAsync(new GetUserRequest { Id = create.Id }, headers);
    });

    Console.WriteLine($"[Result] Id={result.Id}, Name={result.Name}, Age={result.Age}");
}
catch (RpcException ex)
{
    Console.WriteLine($"[Fail] 최종 실패: {ex.Status.Detail}");
}


var update = await client.UpdateUserAsync(new UpdateUserRequest { Id = create.Id, Name = "Alice Kim", Age = 26 }, headers);
Console.WriteLine($"[Update] Id={update.Id}, Name={update.Name}, Age={update.Age}");

var delete = await client.DeleteUserAsync(new DeleteUserRequest { Id = create.Id }, headers);
Console.WriteLine($"[Delete] Success={delete.Success}");



/*

    gRPC 클라이언트가 서버 오류(Unavailable, DeadlineExceeded, 등) 발생 시
    자동으로 재시도(Retry) 하도록 설정하는 구조를 구현.
    서버 장애/순단 시 자동 복구

    클라이언트 설정만으로 가능 (RetryPolicy)
*/