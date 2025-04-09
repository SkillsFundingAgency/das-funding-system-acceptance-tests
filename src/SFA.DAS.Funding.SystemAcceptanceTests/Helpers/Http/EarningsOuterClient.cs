using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using Newtonsoft.Json;
using System.Net;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

internal class EarningsOuterClient
{
    private HttpClient _apiClient;
    private readonly string _subscriptionKey;

    public EarningsOuterClient()
    {
        var config = Configurator.GetConfiguration();
        var baseUrl = config.EarningsOuterApiBaseUrl;

        _apiClient = HttpClientProvider.GetClient(baseUrl);
        _subscriptionKey = config.EarningsOuterSubscriptionKey;
    }

    public async Task<List<FM36Learner>> GetFm36Block(long ukprn, int collectionYear, byte collectionPeriod)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/earnings/learners/{ukprn}/{collectionYear}/{collectionPeriod}");
        request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
        request.Headers.Add("Cache-Control", "no-cache");
        request.Headers.Add("X-Version", "1");
        var response = await _apiClient.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Expected HTTP 200 OK response from GetFm36Block request, but got {response.StatusCode}");

        return JsonConvert.DeserializeObject<List<FM36Learner>>(await response.Content.ReadAsStringAsync())!;
    }
}