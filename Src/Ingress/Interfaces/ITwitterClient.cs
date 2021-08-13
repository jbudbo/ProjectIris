using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ingress.Interfaces
{
    interface ITwitterClient
    {
        Task StartAsync(Uri uri, Func<string, Task> OnTweet, CancellationToken cancellationToken);

        void Drop();
    }
}
