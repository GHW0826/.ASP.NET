using AutoMapper;
using Grpc.Core;
using BIStreamingChat.Models.Dto;
using BIStreamingChat.Repository;
using BIStreamingChat.Repositories;
using BIStreamingChat;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;

namespace BIStreamingChat.Services;

public class BIChatService : BIStreamingChat.BIChatService.BIChatServiceBase
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
                        // Ŭ���̾�Ʈ ���� �� ����
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