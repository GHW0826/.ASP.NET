using Grpc.Core.Interceptors;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GrpcCompression.Auth;

public class JwtInterceptor : Interceptor
{
    private readonly IConfiguration _config;

    public JwtInterceptor(IConfiguration config)
    {
        _config = config;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var token = context.RequestHeaders.GetValue("authorization")?.Replace("Bearer ", "");
        if (string.IsNullOrWhiteSpace(token))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing token"));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var handler = new JwtSecurityTokenHandler();

        try
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true
            }, out _);
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
        }

        return await continuation(request, context);
    }
}