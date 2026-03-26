using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ShortCourseApprovalSteps(ScenarioContext context, EarningsSqlClient earningsSqlClient)
{
    [Given(@"the short course is approved")]
    [When(@"the short course is approved")]
    public async Task WhenTheShortCourseIsApproved()
    {
        var testData = context.Get<TestData>();
        var shortCourseOnProgramme = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single();

        var apprenticeshipCreatedEvent = new SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent
        {
            ApprenticeshipId = TestIdentifierProvider.GetNextApprovalsApprenticeshipId(),
            TrainingCode = shortCourseOnProgramme.CourseCode,
            ActualStartDate = shortCourseOnProgramme.StartDate,
            StartDate = shortCourseOnProgramme.StartDate,
            EndDate = shortCourseOnProgramme.ExpectedEndDate,
            PriceEpisodes = new[]
            {
                new SFA.DAS.CommitmentsV2.Messages.Events.PriceEpisode
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
            LearningType = SFA.DAS.CommitmentsV2.Messages.Events.LearningType.ApprenticeshipUnit,
            TrainingCourseVersion = "1.0",
            ApprenticeshipHashedId = "ABC123",
            AccountLegalEntityId = 12345,
            TransferSenderId = null
        };

        testData.CommitmentsApprenticeshipCreatedEvent = apprenticeshipCreatedEvent;

        await TestServiceBus.Das.SendApprenticeshipApprovedMessage(apprenticeshipCreatedEvent);

        await WaitHelper.WaitForIt(() =>
        {
            var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());

            if ((earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile.IsApproved).GetValueOrDefault())
            {
                testData.ApprovedShortCourseLearningKey = earningsModel.LearningKey;
                return true;
            }

            return false;
        }, "Failed to find approved short course earnings entity.");
    }
}
