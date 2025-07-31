using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public class EarningsInnerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly FundingConfig _fundingConfig;
    private readonly AzureTokenHelper _azureTokenHelper;
    private DateTime _bearerTokenExpiry;
    private string? _azureToken;
    private string? _cachedBearerToken;
    
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

        EnsureBearerToken();
        await EnsureAzureToken();
        var response = await _httpClient.SendAsync(requestMessage);
        return response;
    }

    /// <summary>
    /// Sends a PATCH request to the earnings inner API to save learning support data for an apprenticeship.
    /// </summary>
    public async Task<HttpResponseMessage> SaveLearningSupport(Guid apprenticeshipKey, List<LearningSupportPaymentDetail> request)
    {
        var url = $"apprenticeship/{apprenticeshipKey}/learningSupport";

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = jsonContent
        };

        EnsureBearerToken();
        await EnsureAzureToken();
        var response = await _httpClient.SendAsync(requestMessage);
        return response;
    }

    /// <summary>
    /// Sends a PATCH request to the earnings inner API to save Maths and English data for an apprenticeship.
    /// </summary>

    public async Task<HttpResponseMessage> SaveMathAndEnglishDetails(Guid apprenticeshipKey, List<MathAndEnglishDetails> request)
    {
        var url = $"apprenticeship/{apprenticeshipKey}/mathsAndEnglish";

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = jsonContent
        };

        EnsureBearerToken();
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

    private void EnsureBearerToken()
    {
        if (string.IsNullOrEmpty(_cachedBearerToken) || DateTime.UtcNow >= _bearerTokenExpiry)
        {
            AddBearerToken();
        }
    }

    private void AddBearerToken()
    {
        var claims = GetClaims();
        var signingKey = _fundingConfig.LearningServiceBearerTokenSigningKey;

        var accessToken = ServiceBearerTokenProvider.GetServiceBearerToken(signingKey);

        accessToken = BearerTokenHelper.AddClaimsToBearerToken(accessToken, claims, signingKey);

        _cachedBearerToken = accessToken;
        _bearerTokenExpiry = DateTime.UtcNow.AddMinutes(20);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cachedBearerToken);
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