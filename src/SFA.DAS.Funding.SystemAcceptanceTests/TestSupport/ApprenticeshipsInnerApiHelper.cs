using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    public class ApprenticeshipsInnerApiHelper(ScenarioContext context)
    {
        private readonly ApprenticeshipsInnerApiClient _apiClient = new();

        public async Task CreatePriceChangeRequest(decimal trainingPrice, decimal assessmentPrice, DateTime effectiveFromDate)
        {
            var apprenticeshipCreatedEvent = context.Get<ApprenticeshipCreatedEvent>();

            var requestBody = new ApprenticeshipsInnerApiClient.CreateApprenticeshipPriceChangeRequest
            {
                TrainingPrice = trainingPrice,
                AssessmentPrice = assessmentPrice,
                TotalPrice = trainingPrice + assessmentPrice,
                EffectiveFromDate = effectiveFromDate,
                Reason = "",
                Initiator = "Employer",
                UserId = "SystemAcceptanceTests",
            };

            var response = await _apiClient.PostAsync($"{apprenticeshipCreatedEvent.ApprenticeshipKey}/priceHistory", requestBody);

            if (!response.IsSuccessStatusCode)
            {

            }

        }

        public async Task ApprovePendingPriceChangeRequest(decimal trainingPrice, DateTime approvedDate)
        {
            var apprenticeshipCreatedEvent = context.Get<ApprenticeshipCreatedEvent>();

            var requestBody = new ApprenticeshipsInnerApiClient.ApprovePriceChangeRequest
            {
                TrainingPrice = trainingPrice,
                UserId = "SystemAcceptanceTests",
            };

            var response = await _apiClient.PatchAsync($"{apprenticeshipCreatedEvent.ApprenticeshipKey}/priceHistory/pending", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                
            }
        }
    }
}
