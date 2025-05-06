using Grpc.Net.Compression;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcClient;


public class GzipCompressionProvider : ICompressionProvider
{
    public string EncodingName => "gzip";
    public Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel) =>
        new GZipStream(stream, compressionLevel ?? CompressionLevel.Fastest, leaveOpen: true);

    public Stream CreateDecompressionStream(Stream stream) =>
        new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
};
public class AuthUser
{
    public string UserId { get; set; } = "";
    public string Role { get; set; } = "User";
}

public class LoginResponse
{
    public string token { get; set; } = "";
};
