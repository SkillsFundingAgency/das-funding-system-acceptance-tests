using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using CMT = SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class ApprenticeshipEventHelper
{
    internal static CMT.ApprenticeshipCreatedEvent CreateApprenticeshipCreatedMessageWithCustomValues(this ScenarioContext context, DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode)
    {
        var fixture = new Fixture();
        var testData = context.Get<TestData>();
        var scenarioTitle = context.ScenarioInfo.Title.Length > 50 ? context.ScenarioInfo.Title.Substring(0, 50) : context.ScenarioInfo.Title;

        return fixture.Build<CMT.ApprenticeshipCreatedEvent>()
           .With(_ => _.StartDate, new DateTime(actualStartDate.Year, actualStartDate.Month, 1))
           .With(_ => _.ActualStartDate, actualStartDate)
           .With(_ => _.FirstName, "Sys acceptance test")
           .With(_ => _.LastName, scenarioTitle)
           .With(_ => _.EndDate, plannedEndDate)
           .With(_ => _.PriceEpisodes, new PriceEpisodeHelper().CreateSinglePriceEpisodeUsingStartDate(actualStartDate, agreedPrice))
           .With(_ => _.Uln, testData.Uln)
           .With(_ => _.TrainingCode, trainingCode)
           .With(_ => _.ApprenticeshipEmployerTypeOnApproval, ApprenticeshipEmployerType.Levy)
           .With(_ => _.AccountId, 3871)
           .With(_ => _.TransferSenderId, (long?)null)
           .With(_ => _.DateOfBirth, DateTime.Now.AddYears((-25)).Date)
           .With(_ => _.IsOnFlexiPaymentPilot, true)
           .With(_ => _.TrainingCourseVersion, "1.0")
           .With(_ => _.ProviderId, Constants.UkPrn)
           .With(_ => _.ApprenticeshipId, TestIdentifierProvider.GetNextApprovalsApprenticeshipId())
           .Create();
    }

    internal static CMT.ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithDoB(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, DateTime dob)
    {
        apprenticeshipCreatedEvent.DateOfBirth = dob;
        return apprenticeshipCreatedEvent;
    }

    internal static CMT.ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithEmployerType(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, ApprenticeshipEmployerType employerType)
    {
        apprenticeshipCreatedEvent.ApprenticeshipEmployerTypeOnApproval = employerType;
        return apprenticeshipCreatedEvent;
    }

    internal static async Task PublishApprenticeshipApprovedMessage(this ScenarioContext context, CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        var learningSqlClient = new LearningSqlClient();
        var earningsSqlClient = new EarningsSqlClient();

        var testData = context.Get<TestData>();

        // Some Episodes do not have an earnings profile, so we only check Approval for those that do
        var unapprovedEarningsEpisodeKeys = earningsSqlClient.GetUnapprovedEarningsEpisodeKeys(apprenticeshipCreatedEvent.Uln);

        await TestServiceBus.Das.SendApprenticeshipApprovedMessage(apprenticeshipCreatedEvent);

        var approvedEpisodeKey = Guid.Empty;

        await WaitHelper.WaitForIt(() =>
        {
            var learning = learningSqlClient.TryGetApprenticeshipByUln(apprenticeshipCreatedEvent.Uln);
            var episode = learning?.Episodes.FirstOrDefault(e => e.ApprovalsApprenticeshipId == apprenticeshipCreatedEvent.ApprenticeshipId && e.isApproved);
            if (episode is null)
            {
                return false;
            }
            testData.LearningKey = learning!.Key;
            approvedEpisodeKey = episode.Key;
            return true;
        }, "Failed to find approved episode in Learning");

        if (unapprovedEarningsEpisodeKeys.Contains(approvedEpisodeKey))
        {
            await WaitHelper.WaitForIt(() => earningsSqlClient.EarningsProfileIsApproved(approvedEpisodeKey),
                "Failed to find approved earnings profile in Earnings");
        }
    }
}