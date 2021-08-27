namespace Ingress.Interfaces
{
    public interface ITwitterClient
    {
        Task StartAsync(Uri uri, Func<string, Task> OnTweet, CancellationToken cancellationToken = default);
    }
}
