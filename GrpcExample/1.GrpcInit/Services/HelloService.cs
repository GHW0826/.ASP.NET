using Grpc.Core;
using GrpcInit;

namespace GrpcInit.Services;

public class HelloService : GrpcInit.HelloService.HelloServiceBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var reply = new HelloReply
        {
            Message = $"Hello, {request.Name}!"
        };
        return Task.FromResult(reply);
    }
}
