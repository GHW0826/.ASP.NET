using Grpc.Core.Interceptors;
using Grpc.Core;

namespace GlobalRpcException.Interceptors;

public class ExceptionInterceptor : Interceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException) // gRPC 내부에서 이미 포맷된 예외
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "예외 발생 - Method: {Method}", context.Method);

            // 전역 RpcException으로 래핑
            throw new RpcException(new Status(StatusCode.Internal, "서버 처리 중 오류가 발생했습니다."));
        }
    }
}