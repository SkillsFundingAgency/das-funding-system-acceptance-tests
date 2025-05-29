using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public class EarningsInnerApiHelper
{
    private readonly EarningsInnerApiClient _apiClient = new();

    public async Task MarkAsCareLeaver(Guid apprenticeshipKey)
    {
        var request = new EarningsInnerApiClient.SaveCareDetailsRequest
        {
            HasEHCP = true,
            IsCareLeaver = true,
            CareLeaverEmployerConsentGiven = true
        };

        var response = await _apiClient.SaveCareDetails(apprenticeshipKey, request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetLearningSupportPayments(Guid apprenticeshipKey, List<EarningsInnerApiClient.LearningSupportPaymentDetail> learningSupportDetails)
    {
        var response = await _apiClient.SaveLearningSupport(apprenticeshipKey, learningSupportDetails);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetMathAndEnglishLearning(Guid apprenticeshipKey, List<EarningsInnerApiClient.MathAndEnglishDetails> mathAndEnglishDetails)
    {
        var response = await _apiClient.SaveMathAndEnglishDetails(apprenticeshipKey, mathAndEnglishDetails);
        response.EnsureSuccessStatusCode();
    }
}