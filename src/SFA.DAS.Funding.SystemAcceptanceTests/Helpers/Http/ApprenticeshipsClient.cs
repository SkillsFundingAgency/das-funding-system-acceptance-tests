using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

internal class ApprenticeshipsClient
{
    private HttpClient _apiClient;
    private readonly string _functionKey;

    public ApprenticeshipsClient()
    {
        var config = Configurator.GetConfiguration();
        var baseUrl = config.ApprenticeshipAzureFunctionBaseUrl;
        _apiClient = HttpClientProvider.GetClient(baseUrl);
        _functionKey = config.ApprenticeshipAzureFunctionKey;
    }

    public async Task WithdrawApprenticeship(WithdrawApprenticeshipRequestBody body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/WithdrawApprenticeship?code={_functionKey}");
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        var response = await _apiClient.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Expected HTTP 200 OK response from Withdrawal request, but got {response.StatusCode}");
    }
}

internal class WithdrawApprenticeshipRequestBody
{
    public long UKPRN { get; set; }
    public string ULN { get; set; }
    public string Reason { get; set; }
    public string ReasonText { get; set; }
    public DateTime LastDayOfLearning { get; set; }
    public string ProviderApprovedBy { get; set; }
}