using Grpc.Core.Interceptors;
using Grpc.Core;
using Prometheus;

namespace GrpcHealthcheck.Interceptors;

public class GrpcMetricsInterceptor : Interceptor
{
    private static readonly Counter GrpcCallCounter =
        Metrics.CreateCounter("grpc_server_calls_total", "Total number of gRPC calls", new CounterConfiguration
        {
            LabelNames = new[] { "method", "status" }
        });

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var method = context.Method;

        try
        {
            var response = await continuation(request, context);
            GrpcCallCounter.WithLabels(method, "OK").Inc();
            return response;
        }
        catch (Exception)
        {
            GrpcCallCounter.WithLabels(method, "ERROR").Inc();
            throw;
        }
    }
}