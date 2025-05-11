using AutoMapper;
using Grpc.Core;

namespace GrpcCompression.Services;

public class FileService : GrpcCompression.FileService.FileServiceBase
{
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

    public FileService()
    {
        Directory.CreateDirectory(_uploadPath);
    }

    public override async Task<UploadReply> Upload(IAsyncStreamReader<FileChunk> requestStream, ServerCallContext context)
    {
        string? fileName = null;
        int chunkCount = 0;
        List<byte> fileBuffer = new();

        await foreach (var chunk in requestStream.ReadAllAsync())
        {
            fileName ??= chunk.FileName;
            fileBuffer.AddRange(chunk.Data.ToByteArray());
            chunkCount++;
        }

        var filePath = Path.Combine(_uploadPath, fileName ?? $"unknown_{Guid.NewGuid()}");
        await File.WriteAllBytesAsync(filePath, fileBuffer.ToArray());

        return new UploadReply { Message = "Upload completed", TotalChunks = chunkCount };
    }

    public override async Task Download(FileRequest request, IServerStreamWriter<FileChunk> responseStream, ServerCallContext context)
    {
        var filePath = Path.Combine(_uploadPath, request.FileName);
        if (!File.Exists(filePath))
        {
            throw new RpcException(new Status(StatusCode.NotFound, "File not found"));
        }

        const int chunkSize = 1024 * 64; // 64KB
        var buffer = new byte[chunkSize];
        int index = 0;

        using var fs = File.OpenRead(filePath);
        int bytesRead;
        while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            var chunk = new FileChunk
            {
                FileName = request.FileName,
                Data = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead),
                Index = index++
            };
            await responseStream.WriteAsync(chunk);
        }
    }
}