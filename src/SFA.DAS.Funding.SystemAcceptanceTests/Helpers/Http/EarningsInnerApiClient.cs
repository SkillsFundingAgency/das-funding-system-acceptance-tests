using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public class EarningsInnerApiClient
{
    private readonly HttpClient _httpClient;
    private string _cachedToken;
    private DateTime _tokenExpiry;
    private readonly FundingConfig _fundingConfig;

    public EarningsInnerApiClient()
    {
        _fundingConfig = Configurator.GetConfiguration();
        var baseUrl = _fundingConfig.EarningsInnerApiClientBaseUrl;
        _httpClient = HttpClientProvider.GetClient(baseUrl);
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

        await EnsureBearerToken();
        var response = await _httpClient.SendAsync(requestMessage);
        return response;
    }

    private async Task EnsureBearerToken()
    {
        if (string.IsNullOrEmpty(_cachedToken) || DateTime.UtcNow >= _tokenExpiry)
        {
            await AddBearerToken();
        }
    }

    private async Task AddBearerToken()
    {
        var claims = GetClaims();
        var signingKey = _fundingConfig.ApprenticeshipServiceBearerTokenSigningKey;

        var accessToken = ServiceBearerTokenProvider.GetServiceBearerToken(signingKey);

        accessToken = BearerTokenHelper.AddClaimsToBearerToken(accessToken, claims, signingKey);

        _cachedToken = accessToken;
        _tokenExpiry = DateTime.UtcNow.AddMinutes(20);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cachedToken);
    }

    private Dictionary<string, string> GetClaims()
    {
        return new Dictionary<string, string>
        {
            { "http://schemas.portal.com/ukprn", "88888888" },
            { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "tester" }
        };
    }

    public class SaveCareDetailsRequest
    {
        public bool HasEHCP { get; set; }
        public bool IsCareLeaver { get; set; }
        public bool CareLeaverEmployerConsentGiven { get; set; }
    }
}