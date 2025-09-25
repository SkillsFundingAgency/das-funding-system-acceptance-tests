using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using Newtonsoft.Json;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using System.Net;
using System.Text.Json;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql.LearnerDataSqlClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http
{
    public class LearnerDataOuterApiClient
    {
        private readonly HttpClient _apiClient;
        private readonly string _subscriptionKey;
        private readonly FundingConfig _fundingConfig;
        private DateTime _bearerTokenExpiry;
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
                System.Text.Json.JsonSerializer.Serialize(learnerData),
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

        public async Task<GetLearnerResponse> GetLearners (long ukprn, int academicYear)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/learnerdata/Learners/providers/{ukprn}/academicyears/{academicYear}/learners");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("X-Version", "1");

            var response = await _apiClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

            }

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<GetLearnerResponse>(await response.Content.ReadAsStringAsync())!;
        }

        public async Task UpdateLearning(long ukprn, Guid learningKey, UpdateLearnerRequest learningData)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/learnerdata/providers/{ukprn}/learning/{learningKey}");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("X-Version", "1");

            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(learningData),
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

        private readonly object _tokenLock = new object();

        public async Task<List<FM36Learner>> GetFm36Block(long ukprn, int collectionYear, byte collectionPeriod)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/learnerdata/Learners/providers/{ukprn}/collectionPeriod/{collectionYear}/{collectionPeriod}/fm36data");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("X-Version", "1");
            var response = await _apiClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Expected HTTP 200 OK response from GetFm36Block request, but got {response.StatusCode}");

            return JsonConvert.DeserializeObject<List<FM36Learner>>(await response.Content.ReadAsStringAsync())!;
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
            public UpdateLearnerRequestDeliveryDetails Delivery { get; set; } = new ();
        }
        public class UpdateLearnerRequestDeliveryDetails
        {
            public OnProgramme OnProgramme { get; set; } = new OnProgramme();

            public List<EnglishAndMaths> EnglishAndMaths { get; set; } = [];
        }

        public class OnProgramme
        {
            public List<CostDetails> Costs { get; set; } = new List<CostDetails> ();
            public DateTime? CompletionDate { get; set; }

            public List<LearningSupport> LearningSupport { get; set; } = new List<LearningSupport>();
        }

        public class CostDetails
        {
            public int TrainingPrice { get; set; }
            public int? EpaoPrice { get; set; }
            public DateTime? FromDate { get; set; }
        }

        public class EnglishAndMaths
        {
            public string Course { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
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

        public class Learning
        {
            public string Uln { get; set; } = "";
            public Guid Key { get; set; }
        }

        public class GetLearnerResponse
        {
            public List<Learning> Learners { get; set; } = [];
            public int Total {  get; set; }
            public int Page {  get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }
    }
}
