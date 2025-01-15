namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;


/// <summary>
/// In the absence of DI this is used to prevent multiple instances of the same ApiClient being created
/// </summary>
internal static class HttpClientProvider
{
    private static Dictionary<string, HttpClient> _apiClients = new Dictionary<string, HttpClient>();

    internal static HttpClient GetClient(string baseUrl)
    {
        if (_apiClients.TryGetValue(baseUrl, out var client))
        {
            return client;
        }

        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(baseUrl);

        _apiClients.Add(baseUrl, httpClient);

        return httpClient;
    }
}