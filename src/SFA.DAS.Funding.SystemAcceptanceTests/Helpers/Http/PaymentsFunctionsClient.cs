namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

internal class PaymentsFunctionsClient
{
    private readonly HttpClient _apiClient;
    private readonly string _code;

    public PaymentsFunctionsClient()
    {
        var config = Configurator.GetConfiguration();
        var baseUrl = config.PaymentsFunctionsBaseUrl;
        _apiClient = HttpClientProvider.GetClient(baseUrl);
        _code = config.PaymentsFunctionKey;
    }

    public async Task InvokeReleasePaymentsHttpTrigger(byte collectionPeriod, short collectionYear)
    {
        var path = $"/api/releasePayments/{collectionYear}/{collectionPeriod}";
        var response = await _apiClient.PostAsync($"{path}?code={_code}", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Payments Http Trigger failed. {response.StatusCode} -  {response.ReasonPhrase}\nBaseUrl: {_apiClient.BaseAddress}\nPath:{path}");
        }
    }
}