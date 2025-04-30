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
}