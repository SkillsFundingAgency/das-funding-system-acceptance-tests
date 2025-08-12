using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Common;

[Binding]
/// <summary>
/// This class will 'approve' the apprenticeship commitment therefore creating it in the das-apprenticeship,
/// funding-earnings and funding-payments. 
/// The configuration of the apprenticeship is done in the ConfigureApprenticeshipStepDefinition class before
/// the approve is called.
/// </summary>
public class ApproveApprenticeshipStepDefinition
{

    private readonly ScenarioContext _context;
    private readonly EarningsSqlClient _earningsSqlClient;

    public ApproveApprenticeshipStepDefinition(ScenarioContext context, EarningsSqlClient earningsSqlClient)
    {
        _context = context;
        _earningsSqlClient = earningsSqlClient;
    }

    [Given(@"the apprenticeship commitment is approved")]
    [Given(@"the learning is approved")]
    [When(@"the apprenticeship commitment is approved")]
    [When(@"the learning is approved")]
    public async Task TheApprenticeshipCommitmentIsApproved()
    {
        await ApproveApprenticeshipCommitment();
    }

    public async Task ApproveApprenticeshipCommitment()
    {
        var testData = _context.Get<TestData>();

        await _context.PublishApprenticeshipApprovedMessage(testData.CommitmentsApprenticeshipCreatedEvent);

        Thread.Sleep(5000); // Without this a whole load of tests fail, need to investigate further

        var deliveryPeriods = testData.EarningsGeneratedEvent.DeliveryPeriods;

        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = _earningsSqlClient.GetEarningsEntityModel(_context);
            if (earningsApprenticeshipModel != null)
            {
                return true;
            }
            return false;
        }, "Failed to find Earnings Entity");

        testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile.EarningsProfileId;

        testData.InitialEarningsProfileId = earningsApprenticeshipModel!.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate)!.StartDate)!.EarningsProfile.EarningsProfileId;
        testData.LearningKey = testData.EarningsGeneratedEvent.ApprenticeshipKey;
    }
}
