using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Ingress.Interfaces
{
    public interface ITwitterClient
    {
        Task StartAsync(Uri uri, CancellationToken cancellationToken = default);

        ChannelReader<string> Reader { get; }
    }
}
