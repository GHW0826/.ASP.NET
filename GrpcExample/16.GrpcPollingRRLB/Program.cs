using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using GrpcClient;
using GrpcPollingRRLB;
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

var manager = new GrpcChannelManager(
    "https://localhost:7007",
    "https://localhost:7008"
);
for (int i = 0; i < 5; ++i)
{
    var rrChannel = manager.GetNextChannel();
    var rrClient = new ChatService.ChatServiceClient(rrChannel);
    using var call = rrClient.ServerStream(new Message
    {
        Sender = "Client",
        Content = $"Request {i}"
    }, headers);
    await foreach (var reply in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"[#{i}] {reply.Content}");
    };
}

/*
    여러 개의 gRPC 서버 주소를 라운드로빈 방식으로 분산 호출하는 클라이언트 구조를 구성.

    서버 주소를 여러 개 등록
    요청 시마다 채널을 바꿔가며 사용
    실전 분산 구조/로드밸서 환경에 대비

    1. 서버가 2개 이상 떠있다는 전제
        로컬에서 테스트할 경우:
        포트만 다르게 실행된 gRPC 서버가 2개 이상 존재해야 합니다.
*/