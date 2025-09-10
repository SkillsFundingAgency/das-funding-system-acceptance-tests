using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using Newtonsoft.Json;
using System.Net;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public class EarningsOuterClient
{
    private HttpClient _apiClient;
    private readonly string _subscriptionKey;

    public EarningsOuterClient()
    {
        var config = Configurator.GetConfiguration();
        var baseUrl = config.OuterApiBaseUrl;

        _apiClient = HttpClientProvider.GetClient(baseUrl);
        _subscriptionKey = config.EarningsOuterSubscriptionKey;
    }
}