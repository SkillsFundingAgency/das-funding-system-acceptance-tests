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
        var request = new HttpRequestMessage(HttpMethod.Post, $"api-stub/save?httpMethod={verb}&url={StubUrl(url)}&httpStatusCode={(int)httpStatusCode}");
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        var response = await _apiClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMockResponse(string url, string verb = "Get")
    {
        var response = await _apiClient.DeleteAsync($"api-stub/delete?httpMethod={verb}&url={StubUrl(url)}");
        response.EnsureSuccessStatusCode();
    }

    // The stubbed URL travels as a single query parameter, so its own query string must be escaped
    // or everything after its first "&" is read as a separate parameter of the save/delete request
    private static string StubUrl(string url) => Uri.EscapeDataString($"/{url}");
}
