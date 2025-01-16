using Newtonsoft.Json;
using System.Text;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

/// <summary>
/// This class is used to create mock responses for the WireMock server
/// </summary>
internal class WireMockClient
{
    private HttpClient _apiClient;

    public WireMockClient()
    {
        try
        {
            var connectionString = Configurator.GetConfiguration().WireMockBaseUrl;
            _apiClient = HttpClientProvider.GetClient(connectionString);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create WireMockClient", ex);
        }
    }

    public async Task CreateMockResponse(string url, object body, string verb = "Get")
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"api-stub/save?httpMethod={verb}&url=/{url}");
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await _apiClient.SendAsync(request);
        }
        catch(Exception ex)
        {
            throw new Exception($"Failed to create mock response for {url}", ex);
        }
    }
}
