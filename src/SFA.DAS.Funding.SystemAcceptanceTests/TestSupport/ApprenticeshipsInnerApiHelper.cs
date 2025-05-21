using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public class ApprenticeshipsInnerApiHelper()
{
    private readonly ApprenticeshipsInnerApiClient _apiClient = new();

    public async Task CreatePriceChangeRequest(ScenarioContext context, decimal trainingPrice, decimal assessmentPrice, DateTime effectiveFromDate)
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
        response.EnsureSuccessStatusCode();
    }

    public async Task ApprovePendingPriceChangeRequest(ScenarioContext context, decimal trainingPrice, decimal assessmentPrice, DateTime approvedDate)
    {
        var apprenticeshipCreatedEvent = context.Get<ApprenticeshipCreatedEvent>();

        var requestBody = new ApprenticeshipsInnerApiClient.ApprovePriceChangeRequest
        {
            TrainingPrice = trainingPrice,
            AssessmentPrice = assessmentPrice,
            UserId = "SystemAcceptanceTests",
        };

        var response = await _apiClient.PatchAsync($"{apprenticeshipCreatedEvent.ApprenticeshipKey}/priceHistory/pending", requestBody);
        response.EnsureSuccessStatusCode();
    }
}
