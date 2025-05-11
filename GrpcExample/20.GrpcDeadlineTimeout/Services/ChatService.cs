using AutoMapper;
using Grpc.Core;
using GrpcDeadlineTimeout.Models.Dto;
using GrpcDeadlineTimeout.Repository;
using GrpcDeadlineTimeout.Repositories;
using GrpcDeadlineTimeout;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;

namespace GrpcDeadlineTimeout.Services;

public class ChatService : GrpcDeadlineTimeout.ChatService.ChatServiceBase
{
    public override async Task ServerStream(Message request, IServerStreamWriter<Message> responseStream, ServerCallContext context)
    {
        for (int i = 0; i < 5; ++i)
        {
            await Task.Delay(500);
            await responseStream.WriteAsync(new Message
            {
                Sender = "Server",
                Content = $"Hello {request.Sender}, Message {i}",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
    }

    public override async Task<Ack> ClientStream(IAsyncStreamReader<Message> requestStream, ServerCallContext context)
    {
        await foreach (var msg in requestStream.ReadAllAsync())
        {
            Console.WriteLine($"[From {msg.Sender}] {msg.Content}");
        }
        return new Ack { Status = "All received" };
    }

    public override async Task BiStream(IAsyncStreamReader<Message> requestStream, IServerStreamWriter<Message> responseStream, ServerCallContext context)
    {
        await foreach (var msg in requestStream.ReadAllAsync())
        {
            Console.WriteLine($"[Bi] {msg.Sender}: {msg.Content}");
            await responseStream.WriteAsync(new Message
            {
                Sender = "Server",
                Content = $"Echo: {msg.Content}",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
    }
}