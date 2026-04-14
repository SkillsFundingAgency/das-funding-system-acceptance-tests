using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.ShortCourse;

[Binding]
public class ShortCourseApprovalSteps(ScenarioContext context, EarningsSqlClient earningsSqlClient)
{
    private Fixture _fixture = new Fixture();

    [Given(@"the short course is approved")]
    [When(@"the short course is approved")]
    public async Task WhenTheShortCourseIsApproved()
    {
        var testData = context.Get<TestData>();
        testData.IsShortCourseApproved = true;
        var shortCourseOnProgramme = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single();

        var apprenticeshipCreatedEvent = CreateApprenticeshipCreatedEvent(testData, shortCourseOnProgramme, "ABC123", _fixture.Create<ApprenticeshipEmployerType>());

        testData.CommitmentsApprenticeshipCreatedEvent = apprenticeshipCreatedEvent;

        await TestServiceBus.Das.SendApprenticeshipApprovedMessage(apprenticeshipCreatedEvent);

        await WaitHelper.WaitForIt(() =>
        {
            var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());

            if ((earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile.IsApproved).GetValueOrDefault())
            {
                testData.ShortCourseLearningKey = earningsModel.LearningKey;
                return true;
            }

            return false;
        }, "Failed to find approved short course earnings entity.");
    }

    [Given(@"the short course is not approved")]
    [When(@"the short course is not approved")]
    public async Task WhenTheShortCourseIsNotApproved()
    {
        var testData = context.Get<TestData>();
        
        await WaitHelper.WaitForIt(() =>
        {
            var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());

            if (earningsModel != null)
            {
                // Cache the learning key without approving so it's available in the assertions
                testData.ShortCourseLearningKey = earningsModel.LearningKey;
                return true;
            }

            return false;
        }, "Failed to find short course earnings entity.");
    }

    [When(@"both short courses are approved")]
    public async Task WhenBothShortCoursesAreApproved()
    {
        var testData = context.Get<TestData>();
        testData.IsShortCourseApproved = true;
        
        var firstCourseOnProgramme = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single();
        var firstApprenticeshipCreatedEvent = CreateApprenticeshipCreatedEvent(testData, firstCourseOnProgramme, "ABC123");
        await TestServiceBus.Das.SendApprenticeshipApprovedMessage(firstApprenticeshipCreatedEvent);

        var secondCourseRequest = context.Get<Helpers.Http.LearnerDataOuterApiClient.ShortCourseRequest>("SecondaryShortCourseRequest");
        var secondCourseOnProgramme = secondCourseRequest.Delivery.OnProgramme.Single();
        var secondApprenticeshipCreatedEvent = CreateApprenticeshipCreatedEvent(testData, secondCourseOnProgramme, "XYZ987");
        await TestServiceBus.Das.SendApprenticeshipApprovedMessage(secondApprenticeshipCreatedEvent);

        await WaitHelper.WaitForIt(() =>
        {
            var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());

            if ((earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile.IsApproved).GetValueOrDefault())
            {
                testData.ShortCourseLearningKey = earningsModel.LearningKey;
                return true;
            }

            return false;
        }, "Failed to find approved short course earnings entity.");
    }

    private CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent CreateApprenticeshipCreatedEvent(TestData testData, Helpers.Http.LearnerDataOuterApiClient.ShortCourseOnProgramme shortCourseOnProgramme, string apprenticshipHashedId, ApprenticeshipEmployerType employerType = ApprenticeshipEmployerType.Levy)
    {
        return new CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent
        {
            ApprenticeshipId = TestIdentifierProvider.GetNextApprovalsApprenticeshipId(),
            TrainingCode = shortCourseOnProgramme.CourseCode,
            ActualStartDate = shortCourseOnProgramme.StartDate,
            StartDate = shortCourseOnProgramme.StartDate,
            EndDate = shortCourseOnProgramme.ExpectedEndDate,
            PriceEpisodes = new[]
            {
                new CommitmentsV2.Messages.Events.PriceEpisode
                {
                    FromDate = shortCourseOnProgramme.StartDate,
                    Cost = 2000,
                    TrainingPrice = 2000,
                    EndPointAssessmentPrice = 0
                }
            },
            AccountId = 112,
            Uln = testData.Uln,
            FirstName = "Short",
            LastName = "CourseLearner",
            DateOfBirth = new DateTime(2000, 1, 1),
            ProviderId = Constants.UkPrn,
            LegalEntityName = "Test Legal Entity",
            IsOnFlexiPaymentPilot = true,
            LearningType = CommitmentsV2.Messages.Events.LearningType.ApprenticeshipUnit,
            TrainingCourseVersion = "1.0",
            ApprenticeshipHashedId = apprenticshipHashedId,
            AccountLegalEntityId = 12345,
            TransferSenderId = null,
            ApprenticeshipEmployerTypeOnApproval = employerType
        };
    }
}
