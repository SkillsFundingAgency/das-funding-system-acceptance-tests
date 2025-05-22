using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public class ApprenticeshipsInnerApiHelper()
{
    private readonly ApprenticeshipsInnerApiClient _apiClient = new();

    public async Task CreatePriceChangeRequest(ScenarioContext context, decimal trainingPrice, decimal assessmentPrice, DateTime effectiveFromDate)
    {
        var testData = context.Get<TestData>();

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

        var response = await _apiClient.PostAsync($"{testData.ApprenticeshipKey}/priceHistory", requestBody);
        response.EnsureSuccessStatusCode();
    }

    public async Task ApprovePendingPriceChangeRequest(ScenarioContext context, decimal trainingPrice, decimal assessmentPrice, DateTime approvedDate)
    {
        var testData = context.Get<TestData>();

        var requestBody = new ApprenticeshipsInnerApiClient.ApprovePriceChangeRequest
        {
            TrainingPrice = trainingPrice,
            AssessmentPrice = assessmentPrice,
            UserId = "SystemAcceptanceTests",
        };

        var response = await _apiClient.PatchAsync($"{testData.ApprenticeshipKey}/priceHistory/pending", requestBody);
        response.EnsureSuccessStatusCode();
    }
}
