using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;


/// <summary>
/// In the absence of DI this is used to prevent multiple instances of the same ApiClient being created
/// </summary>
internal static class HttpClientProvider
{
    private static readonly ConcurrentDictionary<string, HttpClient> _apiClients = new();

    internal static HttpClient GetClient(string baseUrl)
    {
        return _apiClients.GetOrAdd(baseUrl, url =>
        {
            var client = new HttpClient { BaseAddress = new Uri(url) };
            return client;
        });
    }
}
