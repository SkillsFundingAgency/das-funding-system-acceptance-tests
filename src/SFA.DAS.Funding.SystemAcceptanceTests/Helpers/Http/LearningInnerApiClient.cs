using System.Net.Http.Headers;
using System.Net.Http.Json;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public class LearningInnerApiClient
{
    private readonly FundingConfig _fundingConfig;
    private readonly HttpClient _httpClient;
    private string _cachedToken;
    private DateTime _tokenExpiry;

    public LearningInnerApiClient()
    {
        _fundingConfig = Configurator.GetConfiguration();
        var baseUrl = _fundingConfig.LearningInnerApiClientBaseUrl;
        _httpClient = HttpClientProvider.GetClient(baseUrl);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, object body)
    {
        EnsureBearerToken();
        return await _httpClient.PostAsJsonAsync(url, body);
    }

    public async Task<HttpResponseMessage> PatchAsync(string url, object body)
    {
        EnsureBearerToken();
        return await _httpClient.PatchAsJsonAsync(url, body);
    }

    private void EnsureBearerToken()
    {
        if (string.IsNullOrEmpty(_cachedToken) || DateTime.UtcNow >= _tokenExpiry)
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

        _cachedToken = accessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(100);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cachedToken);
    }

    private Dictionary<string, string> GetClaims()
    {
        return new Dictionary<string, string>
        {
            { "http://schemas.portal.com/ukprn", $"{Constants.UkPrn}" },
            { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "tester" }
        };
    }

#pragma warning disable CS8618 // Null properties
    public class CreateLearningPriceChangeRequest
    {
        public string Initiator { get; set; }
        public string UserId { get; set; }
        public decimal? TrainingPrice { get; set; }
        public decimal? AssessmentPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Reason { get; set; }
        public DateTime EffectiveFromDate { get; set; }
    }

    public class ApprovePriceChangeRequest
    {
        /// <summary>
        /// Id of the approver, either the provider or the employer
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The training price set by the provider for employer-initiated price changes
        /// </summary>
        public decimal? TrainingPrice { get; set; }

        /// <summary>
        /// The assessment price set by the provider for employer-initiated price changes
        /// </summary>
        public decimal? AssessmentPrice { get; set; }
    }
#pragma warning restore CS8618 // Null properties
}