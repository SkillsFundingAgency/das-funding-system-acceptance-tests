using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.CommitmentsV2.Types;
using APR = SFA.DAS.Apprenticeships.Types;
using CMT = SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class ApprenticeshipEventHelper
{
    internal static CMT.ApprenticeshipCreatedEvent CreateApprenticeshipCreatedMessageWithCustomValues(this ScenarioContext context, DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode)
    {
        var fixture = new Fixture();
        var testData = context.Get<TestData>();

        return fixture.Build<CMT.ApprenticeshipCreatedEvent>()
           .With(_ => _.StartDate, new DateTime(actualStartDate.Year, actualStartDate.Month, 1))
           .With(_ => _.ActualStartDate, actualStartDate)
           .With(_ => _.EndDate, plannedEndDate)
           .With(_ => _.PriceEpisodes, new PriceEpisodeHelper().CreateSinglePriceEpisodeUsingStartDate(actualStartDate, agreedPrice))
           .With(_ => _.Uln, testData.Uln)
           .With(_ => _.TrainingCode, trainingCode)
           .With(_ => _.ApprenticeshipEmployerTypeOnApproval, ApprenticeshipEmployerType.Levy)
           .With(_ => _.AccountId, 3871)
           .With(_ => _.TransferSenderId, (long?)null)
           .With(_ => _.DateOfBirth, DateTime.Now.AddYears((-25)))
           .With(_ => _.IsOnFlexiPaymentPilot, true)
           .With(_ => _.TrainingCourseVersion, "1.0")
           .With(_ => _.ProviderId, Constants.UkPrn)
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
        var testData = context.Get<TestData>();

        await TestServiceBus.Das.SendApprenticeshipApprovedMessage(apprenticeshipCreatedEvent);

        await WaitHelper.WaitForIt(() =>
        {
            APR.ApprenticeshipCreatedEvent? apprenticeshipEvent =
                ApprenticeshipsTypesEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.Uln == apprenticeshipCreatedEvent.Uln).message;
            if (apprenticeshipEvent != null)
            {
                testData.ApprenticeshipCreatedEvent = apprenticeshipEvent;
                return true;
            }
            return false;
        }, "Failed to find published event in apprenticeships");

        await WaitHelper.WaitForIt(() =>
        {
            EarningsGeneratedEvent? earningsEvent =
                EarningsGeneratedEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.Uln == apprenticeshipCreatedEvent.Uln).message;
            if (earningsEvent != null)
            {
                testData.EarningsGeneratedEvent = earningsEvent;
                return true;
            }
            return false;
        }, "Failed to find published event in Earnings");

        testData.ApprenticeshipKey = testData.EarningsGeneratedEvent.ApprenticeshipKey;

    }
}