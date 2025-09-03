using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public class EarningsInnerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly FundingConfig _fundingConfig;
    private readonly AzureTokenHelper _azureTokenHelper;
    private string? _azureToken;
    
    public EarningsInnerApiClient()
    {
        _fundingConfig = Configurator.GetConfiguration();
        var baseUrl = _fundingConfig.EarningsInnerApiClientBaseUrl;
        _httpClient = HttpClientProvider.GetClient(baseUrl);
        _azureTokenHelper = new AzureTokenHelper();
    }

    /// <summary>
    /// Sends a PATCH request to the earnings inner API to save care details for an apprenticeship.
    /// </summary>
    /// <param name="apprenticeshipKey">The apprenticeship key (Guid).</param>
    /// <param name="request">The care details request.</param>
    public async Task<HttpResponseMessage> SaveCareDetails(Guid apprenticeshipKey, SaveCareDetailsRequest request)
    {
        var url = $"apprenticeship/{apprenticeshipKey}/careDetails";

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = jsonContent
        };

        await EnsureAzureToken();
        var response = await _httpClient.SendAsync(requestMessage);
        return response;
    }

    private async Task EnsureAzureToken()
    {
        if (string.IsNullOrEmpty(_azureToken))
        {
            _azureToken = await _azureTokenHelper.GetAccessTokenAsync(_fundingConfig.EarningsInnerApiScope);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _azureToken);
        }
    }

    private Dictionary<string, string> GetClaims()
    {
        return new Dictionary<string, string>
        {
            { "http://schemas.portal.com/ukprn", $"{Constants.UkPrn}" },
            { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "tester" }
        };
    }

    public class SaveCareDetailsRequest
    {
        public bool HasEHCP { get; set; }
        public bool IsCareLeaver { get; set; }
        public bool CareLeaverEmployerConsentGiven { get; set; }
    }

    public class LearningSupportPaymentDetail
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MathAndEnglishDetails
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Course { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? ActualEndDate => CompletionDate; //todo: tweak this when refactored
        public int? PriorLearningAdjustmentPercentage { get; set; }
        public DateTime? WithdrawalDate { get; set; }
    }

}