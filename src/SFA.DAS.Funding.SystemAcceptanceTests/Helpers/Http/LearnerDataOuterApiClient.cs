using System.Text.Json;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http
{
    public class LearnerDataOuterApiClient
    {
        private readonly HttpClient _apiClient;
        private readonly string _subscriptionKey;

        public LearnerDataOuterApiClient()
        {
            var config = Configurator.GetConfiguration();
            var baseUrl = config.LearnerDataOuterApiClientBaseUrl;
            _subscriptionKey = config.LearnerDataOuterApiSubscriptionKey;
            _apiClient = HttpClientProvider.GetClient(baseUrl);
        }

        public async Task AddLearnerData(long ukprn, int academicYear, IEnumerable<LearnerDataRequest> learnerData)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/provider/{ukprn}/{academicYear}/learners");
            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("X-Version", "1");

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(learnerData),
                System.Text.Encoding.UTF8,
                "application/json");

            request.Content = jsonContent;
            var response = await _apiClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
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
    }
}
