using Grpc.Core.Interceptors;
using Grpc.Core;

namespace GrpcStreaming.Interceptors;

public class RoleInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var httpContext = context.GetHttpContext();
        var user = httpContext.User;

        if (context.Method.Contains("AdminOnly") && !user.IsInRole("Admin"))
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Admin 권한 필요"));
        }

        return await continuation(request, context);
    }
}
