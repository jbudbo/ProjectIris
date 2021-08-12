namespace Ingress.Clients
{
    using Interfaces;
    using System.Net.Http;

    class TwitterClient : ITwitterClient
    {
        private readonly HttpClient baseClient;

        public TwitterClient(HttpClient httpClient)
        {
            baseClient = httpClient;
        }
    }
}
