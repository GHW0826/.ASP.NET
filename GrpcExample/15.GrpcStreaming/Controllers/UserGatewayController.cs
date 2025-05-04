using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace GrpcStreaming.Controllers;

[ApiController]
[Route("api/gateway/[controller]")]
public class UserGatewayController : ControllerBase
{
    private readonly GrpcStreaming.Services.UserService _userService;

    public UserGatewayController(GrpcStreaming.Services.UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var grpcRequest = new GetUserRequest { Id = id };
        var context = new ServerCallContextMock(); // 필요 시 가짜 context 구현
        var reply = await _userService.GetUser(grpcRequest, context);
        return Ok(reply);
    }

    public class ServerCallContextMock : ServerCallContext
    {
        public static ServerCallContext Default => new ServerCallContextMock();

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options) => throw new NotImplementedException();
        protected override string MethodCore => "MockMethod";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "localhost";
        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
        protected override Metadata RequestHeadersCore => new Metadata();
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore => new Metadata();
        protected override Status StatusCore { get; set; } = Status.DefaultSuccess;
        protected override WriteOptions WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new AuthContext("mock", new Dictionary<string, List<AuthProperty>>());
    }
}
