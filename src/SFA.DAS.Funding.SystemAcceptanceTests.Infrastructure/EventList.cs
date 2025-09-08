namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure;

public static class EventList
{
    public static IList<string> GetEventTypes()
    {
        var events = new List<string>
        {
            "SFA.DAS.Approvals.EventHandlers.Messages.ApprovalCreatedEvent",
            typeof(SFA.DAS.Funding.ApprenticeshipEarnings.Types.EarningsGeneratedEvent).FullName!,
            typeof(SFA.DAS.Learning.Types.LearningCreatedEvent).FullName!,
            typeof(SFA.DAS.Funding.ApprenticeshipEarnings.Types.ApprenticeshipEarningsRecalculatedEvent).FullName!
        };

        return events;
    }
}