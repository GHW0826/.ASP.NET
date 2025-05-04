using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcPollingRRLB;

public class GrpcChannelManager
{
    private readonly string[] _addresses;
    private int _currentIndex = 0;

    public GrpcChannelManager(params string[] addresses)
    {
        _addresses = addresses;
    }

    public GrpcChannel GetNextChannel()
    {
        var index = Interlocked.Increment(ref _currentIndex);
        var address = _addresses[index % _addresses.Length];

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        return GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            HttpHandler = handler
        });
    }
}