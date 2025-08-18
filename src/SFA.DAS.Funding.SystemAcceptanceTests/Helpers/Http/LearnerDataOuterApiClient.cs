using System.Net.Http.Headers;
using System.Text.Json;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql.LearnerDataSqlClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http
{
    public class LearnerDataOuterApiClient
    {
        private readonly HttpClient _apiClient;
        private readonly string _subscriptionKey;
        private readonly FundingConfig _fundingConfig;
        private DateTime _bearerTokenExpiry;
        private string? _azureToken;
        private string? _cachedBearerToken;

        public LearnerDataOuterApiClient() {
            _fundingConfig = Configurator.GetConfiguration();
            var baseUrl = _fundingConfig.OuterApiBaseUrl;
            _subscriptionKey = _fundingConfig.LearnerDataOuterApiSubscriptionKey;
            _apiClient = HttpClientProvider.GetClient(baseUrl);
        }

        public async Task AddLearnerData(long ukprn, int academicYear, IEnumerable<LearnerDataRequest> learnerData)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/learnerdata/provider/{ukprn}/academicyears/{academicYear}/learners");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("X-Version", "1");

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(learnerData),
                System.Text.Encoding.UTF8,
                "application/json");

            request.Content = jsonContent;
            var response = await _apiClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateLearning(Guid learningKey, UpdateLearnerRequest learningData)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/learnerdata/Learners/{learningKey}");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("X-Version", "1");

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(learningData),
                System.Text.Encoding.UTF8,
                "application/json");

            request.Content = jsonContent;
            EnsureBearerToken();
            var response = await _apiClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
            }

            response.EnsureSuccessStatusCode();
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

            _apiClient.DefaultRequestHeaders.Remove("X-Forwarded-Authorization");
            _apiClient.DefaultRequestHeaders.Add("X-Forwarded-Authorization",
                $"Bearer {_cachedBearerToken}");
        }

        private Dictionary<string, string> GetClaims()
        {
            return new Dictionary<string, string>();
        }


        public class LearnerDataRequest
        {
            public long ULN { get; set; }
            public long UKPRN { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string? LearnerEmail { get; set; }
            public DateTime DateOfBirth { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime PlannedEndDate { get; set; }
            public int? PercentageLearningToBeDelivered { get; set; }
            public int EpaoPrice { get; set; }
            public int TrainingPrice { get; set; }
            public string? AgreementId { get; set; }
            public bool IsFlexiJob { get; set; }
            public int? PlannedOTJTrainingHours { get; set; }
            public int StandardCode { get; set; }
            public string ConsumerReference { get; set; }
        }

        public class UpdateLearnerRequest
        {
            public UpdateLearnerRequestDeliveryDetails Delivery { get; set; } = new UpdateLearnerRequestDeliveryDetails();
        }
        public class UpdateLearnerRequestDeliveryDetails
        {
            public DateTime? CompletionDate { get; set; }
            public OnProgramme OnProgramme { get; set; } = new OnProgramme();

            public List<MathsAndEnglish> MathsAndEnglishCourses { get; set; } = [];
        }

        public class OnProgramme
        {
            public List<LearningSupport> LearningSupport { get; set; } = new List<LearningSupport>();
        }

        public class MathsAndEnglish
        {
            public string Course { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime PlannedEndDate { get; set; }
            public DateTime? CompletionDate { get; set; }
            public DateTime? WithdrawalDate { get; set; }
            public int? PriorLearningPercentage { get; set; }
            public decimal Amount { get; set; }
            public List<LearningSupport> LearningSupport { get; set; } = new List<LearningSupport>();
        }

        public class LearningSupport
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
    }
}
