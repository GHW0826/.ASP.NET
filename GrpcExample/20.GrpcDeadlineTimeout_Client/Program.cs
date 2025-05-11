using FileUploadDownload;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Compression;
using GrpcClient;
using System.IO.Compression;
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


await UploadFileAsync("sample.txt");
await DownloadFileAsync("sample.txt", "downloaded_sample.txt");

var deadline = DateTime.UtcNow.AddSeconds(3); // 3초 제한
var callOptions = new CallOptions(deadline: deadline);

async Task UploadFileAsync(string filePath)
{
    var handler = new SocketsHttpHandler();
    handler.EnableMultipleHttp2Connections = true;
    var channel = GrpcChannel.ForAddress("https://localhost:7007", new GrpcChannelOptions
    {
        HttpHandler = handler,
        CompressionProviders = new List<ICompressionProvider>
        {
            new GrpcClient.GzipCompressionProvider(),
        },
    });
    var client = new FileService.FileServiceClient(channel);

     // JWT or 기타 메타데이터
    var headers = new Metadata
    {
        { "authorization", $"Bearer {token}" }
    };

        // Deadline 설정 (3초)
    var deadline = DateTime.UtcNow.AddSeconds(3);
    var callOptions = new CallOptions(headers: headers, deadline: deadline);

    using var call = client.Upload(callOptions);
    const int chunkSize = 64 * 1024;

    var fileName = Path.GetFileName(filePath);
    using var fs = File.OpenRead(filePath);
    byte[] buffer = new byte[chunkSize];
    int bytesRead;
    int index = 0;

    while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
    {
        var chunk = new FileChunk
        {
            FileName = fileName,
            Data = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead),
            Index = index++
        };
        await call.RequestStream.WriteAsync(chunk);
    }


    await call.RequestStream.CompleteAsync();
    var reply = await call.ResponseAsync;
    Console.WriteLine($"✅ 업로드 완료: {reply.Message} ({reply.TotalChunks} chunks)");
}

async Task DownloadFileAsync(string fileName, string savePath)
{
    var channel = GrpcChannel.ForAddress("https://localhost:7007");
    var client = new FileService.FileServiceClient(channel);

    using var call = client.Download(new FileRequest { FileName = fileName });

    await using var output = File.Create(savePath);
    int chunkCount = 0;

    await foreach (var chunk in call.ResponseStream.ReadAllAsync())
    {
        await output.WriteAsync(chunk.Data.ToByteArray());
        chunkCount++;
    }
    Console.WriteLine($"✅ 다운로드 완료: {chunkCount} chunks 저장됨 → {savePath}");
}