using Newtonsoft.Json;
using System.Text;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

internal class LearningClient
{
    private HttpClient _apiClient;
    private readonly string _functionKey;
    private readonly string _signingKey;

    public LearningClient()
    {
        var config = Configurator.GetConfiguration();
        var baseUrl = config.LearningAzureFunctionBaseUrl;
        _apiClient = HttpClientProvider.GetClient(baseUrl);
        _functionKey = config.LearningAzureFunctionKey;
    }

    public async Task WithdrawLearning(WithdrawLearningRequestBody body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/WithdrawLearning?code={_functionKey}");
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        HttpResponseMessage? response = null;
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
            LoggerHelper.WriteLog($"Withdraw Learning Response Code: {response?.StatusCode}");
        }
    }
}

internal class WithdrawLearningRequestBody
{
    public long UKPRN { get; set; }
    public string ULN { get; set; }
    public string Reason { get; set; }
    public string ReasonText { get; set; }
    public DateTime LastDayOfLearning { get; set; }
    public string ProviderApprovedBy { get; set; }
}