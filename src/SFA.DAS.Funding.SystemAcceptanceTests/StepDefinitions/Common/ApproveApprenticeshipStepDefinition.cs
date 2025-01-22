using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.TestSupport.DcLearnerDataHelper;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;
namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Common;

[Binding]
public class ApproveApprenticeshipStepDefinition
{

    private readonly ScenarioContext _context;
    private readonly ApprenticeshipMessageHandler _messageHelper;
    private readonly EarningsSqlClient _earningsEntitySqlClient;

    public ApproveApprenticeshipStepDefinition(ScenarioContext context)
    {
        _context = context;
        _messageHelper = new ApprenticeshipMessageHandler(_context);
        _earningsEntitySqlClient = new EarningsSqlClient();
    }

    [Given(@"the apprenticeship commitment is approved")]
    [When(@"the apprenticeship commitment is approved")]
    public async Task TheApprenticeshipCommitmentIsApproved()
    {
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
        await _messageHelper.PublishApprenticeshipApprovedMessage(commitmentsApprenticeshipCreatedEvent);

        await MockLearnerDataResponse();

        var earnings = _context.Get<EarningsGeneratedEvent>();
        var deliveryPeriods = earnings.DeliveryPeriods;
        _context.Set(deliveryPeriods);

        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);
            if (earningsApprenticeshipModel != null)
            {
                return true;
            }
            return false;
        }, "Failed to find Earnings Entity");

        var initialEarningsProfileId = earningsApprenticeshipModel.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).EarningsProfile.EarningsProfileId;
        _context.Set(initialEarningsProfileId, ContextKeys.InitialEarningsProfileId);
        _context.Set(earnings.ApprenticeshipKey, ContextKeys.ApprenticeshipKey);
    }

    private async Task MockLearnerDataResponse()
    {
        var wireMockClient = new WireMockClient();
        var learnerData = _context.GetLearner();
        var currentAcademicYear = Convert.ToInt32(TableExtensions.CalculateAcademicYear("CurrentMonth+0"));
        await wireMockClient.CreateMockResponse($"learners/{currentAcademicYear}?ukprn={learnerData.Ukprn}&fundModel=36&progType=-1&standardCode=-1&pageNumber=1&pageSize=1000", new List<Learner> { learnerData });
    }
}
