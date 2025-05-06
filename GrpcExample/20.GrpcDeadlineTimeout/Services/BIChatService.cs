using AutoMapper;
using Grpc.Core;
using GrpcDeadlineTimeout.Models.Dto;
using GrpcDeadlineTimeout.Repository;
using GrpcDeadlineTimeout.Repositories;
using GrpcDeadlineTimeout;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;

namespace GrpcDeadlineTimeout.Services;

public class BIChatService : GrpcDeadlineTimeout.BIChatService.BIChatServiceBase
{
    private static ConcurrentDictionary<string, IServerStreamWriter<BIChatMessage>> _clients = new();

    public override async Task Chat(IAsyncStreamReader<BIChatMessage> requestStream, IServerStreamWriter<BIChatMessage> responseStream, ServerCallContext context)
    {
        var userId = Guid.NewGuid().ToString();
        _clients[userId] = responseStream;

        try
        {
            await foreach (var msg in requestStream.ReadAllAsync())
            {
                Console.WriteLine($"[{msg.User}] {msg.Message}");

                var broadcast = new BIChatMessage
                {
                    User = msg.User,
                    Message = msg.Message,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                foreach (var kv in _clients)
                {
                    try
                    {
                        await kv.Value.WriteAsync(broadcast);
                    }
                    catch
                    {
                        // 클라이언트 종료 시 제거
                        _clients.TryRemove(kv.Key, out _);
                    }
                }
            }
        }
        finally
        {
            _clients.TryRemove(userId, out _);
        }
    }
}