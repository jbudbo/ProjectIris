namespace Ingress.Interfaces
{
    interface ITwitterClient
    {
        Task StartAsync(Uri uri, Func<string, Task> OnTweet, CancellationToken cancellationToken = default);

        void Drop();
    }
}
