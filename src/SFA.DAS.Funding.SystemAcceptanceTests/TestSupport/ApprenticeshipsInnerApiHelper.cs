using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public class ApprenticeshipsInnerApiHelper()
{
    private readonly LearningInnerApiClient _apiClient = new();

    public async Task CreatePriceChangeRequest(ScenarioContext context, decimal trainingPrice, decimal assessmentPrice, DateTime effectiveFromDate)
    {
        var testData = context.Get<TestData>();

        var requestBody = new LearningInnerApiClient.CreateLearningPriceChangeRequest
        {
            TrainingPrice = trainingPrice,
            AssessmentPrice = assessmentPrice,
            TotalPrice = trainingPrice + assessmentPrice,
            EffectiveFromDate = effectiveFromDate,
            Reason = "",
            Initiator = "Employer",
            UserId = "SystemAcceptanceTests",
        };

        var response = await _apiClient.PostAsync($"{testData.LearningKey}/priceHistory", requestBody);
        response.EnsureSuccessStatusCode();
    }

    public async Task ApprovePendingPriceChangeRequest(ScenarioContext context, decimal trainingPrice, decimal assessmentPrice, DateTime approvedDate)
    {
        var testData = context.Get<TestData>();

        var requestBody = new LearningInnerApiClient.ApprovePriceChangeRequest
        {
            TrainingPrice = trainingPrice,
            AssessmentPrice = assessmentPrice,
            UserId = "SystemAcceptanceTests",
        };

        var response = await _apiClient.PatchAsync($"{testData.LearningKey}/priceHistory/pending", requestBody);
        response.EnsureSuccessStatusCode();
    }
}
