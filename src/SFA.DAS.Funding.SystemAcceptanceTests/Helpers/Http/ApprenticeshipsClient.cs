using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

internal class ApprenticeshipsClient
{
    private HttpClient _apiClient;
    private readonly string _functionKey;
    private readonly string _signingKey;

    public ApprenticeshipsClient()
    {
        var config = Configurator.GetConfiguration();
        var baseUrl = config.ApprenticeshipAzureFunctionBaseUrl;
        _apiClient = HttpClientProvider.GetClient(baseUrl);
        _functionKey = config.ApprenticeshipAzureFunctionKey;
        _signingKey = config.ApprenticeshipServiceBearerTokenSigningKey;
    }

    public async Task WithdrawApprenticeship(WithdrawApprenticeshipRequestBody body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/WithdrawApprenticeship?code={_functionKey}");
        request.Headers.Add("ServiceBearerToken", ServiceBearerTokenProvider.GetServiceBearerToken(_signingKey));
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        HttpResponseMessage response = null;
        try
        {
            response = await _apiClient.SendAsync(request);
        }
        catch (Exception e)
        {
            LoggerHelper.WriteLog(e.Message);
            throw;
        }
        finally
        {
            LoggerHelper.WriteLog($"WithdrawApprenticeshipRequest: {System.Text.Json.JsonSerializer.Serialize(request, new JsonSerializerOptions() { WriteIndented = true })}");
            LoggerHelper.WriteLog($"WithdrawApprenticeshipResponse: {System.Text.Json.JsonSerializer.Serialize(response, new JsonSerializerOptions() { WriteIndented = true })}");
        }
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