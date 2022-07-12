using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ingress.Clients;

using Interfaces;
using Models;
using System.Text;

internal sealed class TwitterClient : ITwitterClient, IDisposable
{
    private readonly ILogger<TwitterClient> logger;
    private readonly TwitterOptions options;
    private readonly HttpClient baseClient;

    private HttpResponseMessage responseMessage;
    private Stream responseStream;
    private StreamReader responseReader;

    public TwitterClient(HttpClient client, ILogger<TwitterClient> logger, IOptionsSnapshot<TwitterOptions> options)
    {
        this.logger = logger;
        this.options = options.Value;
        baseClient = client;
    }

    public void Dispose()
    {
        responseReader?.Close();
        responseReader?.Dispose();

        responseStream?.Close();
        responseStream?.Dispose();

        responseMessage?.Dispose();
    }

    public async Task<StreamReader> GetReaderAsync(CancellationToken cancellationToken = default)
    {
        logger.Connecting(options.Endpoint);
        await InitPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        return responseReader;
    }

    private async Task InitPipeReaderAsync(CancellationToken cancellationToken)
    {
        const string FIELDS_KEY = "tweet.fields";
        const string MEDIA_KEY = "media.fields";

        try
        {
            var queryParams = new Dictionary<string, string>
            {
                [FIELDS_KEY] = "entities",
                [MEDIA_KEY] = "type,url"
            };

            string path = $"{options.Endpoint}?{string.Join('&', queryParams.Select(p => $"{p.Key}={p.Value}"))}";

            responseMessage = await baseClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
            {
                logger.LogError("Unsuccessful call to API: {Code} using token {Token}", responseMessage.StatusCode, options.Bearer);
                return;
            }

            responseStream = await responseMessage.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            responseReader = new StreamReader(responseStream, Encoding.UTF8);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error");
        }
    }
}
