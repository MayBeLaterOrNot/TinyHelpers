﻿using System.Web;

namespace TinyHelpers.Http;

public class QueryStringInjectorHttpClientHandler : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, Task<Dictionary<string, string>>> getQueryString;

    public QueryStringInjectorHttpClientHandler(Func<HttpRequestMessage, Task<Dictionary<string, string>>> getQueryString)
    {
        this.getQueryString = getQueryString ?? throw new ArgumentNullException(nameof(getQueryString));
    }

    public QueryStringInjectorHttpClientHandler(Func<HttpRequestMessage, Task<Dictionary<string, string>>> getQueryString, HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
        this.getQueryString = getQueryString ?? throw new ArgumentNullException(nameof(getQueryString));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var queryStringParameters = HttpUtility.ParseQueryString(request.RequestUri!.Query);

        var queryString = await getQueryString(request).ConfigureAwait(false);
        if (queryString is not null)
        {
            foreach (var parameter in queryString)
            {
                queryStringParameters.Add(parameter.Key, parameter.Value);
            }
        }

        var uriBuilder = new UriBuilder(request.RequestUri)
        {
            Query = queryStringParameters.ToString()
        };

        request.RequestUri = new Uri(uriBuilder.ToString());
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
