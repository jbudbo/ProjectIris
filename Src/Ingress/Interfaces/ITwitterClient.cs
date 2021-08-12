using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ingress.Interfaces
{
    interface ITwitterClient
    {
        event EventHandler<string> OnTweet;

        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

        void Drop();
    }
}
