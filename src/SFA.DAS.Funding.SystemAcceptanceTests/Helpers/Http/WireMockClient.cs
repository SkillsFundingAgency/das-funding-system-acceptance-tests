using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

/// <summary>
/// This class is used to create mock responses for the WireMock server
/// </summary>
internal class WireMockClient
{
    private HttpClient _apiClient;

    public WireMockClient()
    {
        var baseUrl = Configurator.GetConfiguration().WireMockBaseUrl;
        _apiClient = HttpClientProvider.GetClient(baseUrl);
    }

    public async Task CreateMockResponse(string url, object body, string verb = "Get", HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api-stub/save?httpMethod={verb}&url=/{url}&httpStatusCode={(int)httpStatusCode}");
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        var response = await _apiClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMockResponse(string url, string verb = "Get")
    {
        var response = await _apiClient.DeleteAsync($"api-stub/delete?httpMethod={verb}&url=/{url}");
        response.EnsureSuccessStatusCode();
    }
}
