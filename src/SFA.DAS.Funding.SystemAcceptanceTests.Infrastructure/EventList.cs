namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure;

public static class EventList
{
    public static IList<string> GetEventTypes()
    {
        var events = new List<string>
        {
            "SFA.DAS.Approvals.EventHandlers.Messages.ApprovalCreatedEvent",
            typeof(ApprenticeshipEarnings.Types.EarningsGeneratedEvent).FullName!,
            typeof(Learning.Types.LearningCreatedEvent).FullName!,
            typeof(ApprenticeshipEarnings.Types.ApprenticeshipEarningsRecalculatedEvent).FullName!,
            typeof(Learning.Types.EndDateChangedEvent).FullName!,
            typeof(Learning.Types.LearningWithdrawnEvent).FullName!,
            typeof(Learning.Types.WithdrawalRevertedEvent).FullName!
        };

        return events;
    }
}