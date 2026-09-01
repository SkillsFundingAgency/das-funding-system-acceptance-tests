using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;
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
    private readonly LearningSqlClient _learningSqlClient;
    private readonly LearnerDataOuterApiHelper _learnerDataOuterApiHelper;

    public ApproveApprenticeshipStepDefinition(ScenarioContext context, EarningsSqlClient earningsSqlClient, LearningSqlClient learningSqlClient, LearnerDataOuterApiHelper learnerDataOuterApiHelper)
    {
        _context = context;
        _earningsSqlClient = earningsSqlClient;
        _learningSqlClient = learningSqlClient;
        _learnerDataOuterApiHelper = learnerDataOuterApiHelper;
    }

    [Given(@"the apprenticeship commitment is approved")]
    [Given(@"the learning is approved")]
    [When(@"the apprenticeship commitment is approved")]
    [When(@"the learning is approved")]
    public async Task TheApprenticeshipCommitmentIsApproved()
    {
        await CreateDraftApprenticeshipAndApproveIt();
    }

    /// <summary>
    /// This method will take the ApprenticeshipCreatedEvent from the ScenarioContext
    /// and "adapt" it into a POSTed Draft Apprenticeship and then approve it.
    /// This allows for the FLP-2012-originated switch away from legacy apprenticeships earnings generation tests
    /// Without having to rewrite every single test in the suite.
    /// </summary>
    /// <returns></returns>
    public async Task CreateDraftApprenticeshipAndApproveIt()
    {
        var testData = _context.Get<TestData>();
        var apprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        var draftLearnerData = new LearnerDataRequest
        {
            ConsumerReference = "AcceptanceTests",
            Learner = new StubLearner
            {
                Uln = apprenticeshipCreatedEvent.Uln,
                LearnerRef = apprenticeshipCreatedEvent.Uln,
                Firstname = apprenticeshipCreatedEvent.FirstName,
                Lastname = apprenticeshipCreatedEvent.LastName,
                Dob = apprenticeshipCreatedEvent.DateOfBirth,
                HasEhcp = false
            },
            Delivery = new StubDelivery
            {
                OnProgramme = new[]
                {
                    new StubOnProgramme
                    {
                        Care = new Care(),
                        StandardCode = int.Parse(apprenticeshipCreatedEvent.TrainingCode),
                        AgreementId = "1",
                        LearnAimRef = "ZPROG001",
                        PercentageOfTrainingLeft = 0,
                        IsFlexiJob = false,
                        StartDate = apprenticeshipCreatedEvent.ActualStartDate,
                        ExpectedEndDate = apprenticeshipCreatedEvent.EndDate,
                        Costs = apprenticeshipCreatedEvent.PriceEpisodes.Select(p => new CostDetails
                        {
                            TrainingPrice = (int?)p.TrainingPrice,
                            EpaoPrice = (int?)p.EndPointAssessmentPrice,
                            FromDate = p.FromDate
                        }).ToList(),
                        LearningSupport = new List<Helpers.Http.LearnerDataOuterApiClient.LearningSupport>()
                    }
                },
                EnglishAndMaths = new List<StubEnglishAndMaths>()
            }
        };

        await _learnerDataOuterApiHelper.AddLearnerData(apprenticeshipCreatedEvent.ProviderId, draftLearnerData);

        await WaitHelper.WaitForIt(() =>
        {
            var learning = _learningSqlClient.TryGetApprenticeshipByUln(apprenticeshipCreatedEvent.Uln);
            if (learning?.Episodes == null || !learning.Episodes.Any(e =>
                    e.Ukprn == apprenticeshipCreatedEvent.ProviderId &&
                    e.TrainingCode.Trim() == apprenticeshipCreatedEvent.TrainingCode))
            {
                return false;
            }

            testData.LearnerKey = learning.LearnerKey;
            return true;
        }, "Failed to find draft apprenticeship in Learning DB");

        await _context.PublishApprenticeshipApprovedMessage(apprenticeshipCreatedEvent);
    }

    //Legacy approval method - retained for now
    //What is this for? Legacy apprenticeship creation?
    // Can test that legacy appprenticeships are still created ok
    // And have no earnings
    public async Task ApproveApprenticeshipCommitment()
    {
        var testData = _context.Get<TestData>();

        await _context.PublishApprenticeshipApprovedMessage(testData.CommitmentsApprenticeshipCreatedEvent);

        //Thread.Sleep(5000); // Without this a whole load of tests fail, need to investigate further

        //var deliveryPeriods = testData.EarningsGeneratedEvent.DeliveryPeriods;

        //EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        //await WaitHelper.WaitForIt(() =>
        //{
        //    earningsApprenticeshipModel = _earningsSqlClient.GetApprenticeshipEarningsEntityModel(_context);
        //    if (earningsApprenticeshipModel != null)
        //    {
        //        return true;
        //    }
        //    return false;
        //}, "Failed to find Earnings Entity");

        //testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent).EarningsProfile.EarningsProfileId;

        //testData.InitialEarningsProfileId = earningsApprenticeshipModel!.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate)!.StartDate)!.EarningsProfile.EarningsProfileId;
        //testData.LearningKey = testData.EarningsGeneratedEvent.ApprenticeshipKey;
        //var learning = _learningSqlClient.GetApprenticeshipByUln(testData.Uln);
        //testData.LearnerKey = learning.LearnerKey;
    }
}
