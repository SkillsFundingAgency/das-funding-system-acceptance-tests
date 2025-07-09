namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure;

public static class EventList
{
    public static IList<string> GetEventTypes()
    {
        var events = new List<string>
        {
            "SFA.DAS.Approvals.EventHandlers.Messages.ApprovalCreatedEvent",
            typeof(SFA.DAS.Funding.ApprenticeshipPayments.Types.PaymentsGeneratedEvent).FullName!,
            typeof(SFA.DAS.Funding.ApprenticeshipPayments.Types.FinalisedOnProgammeLearningPaymentEvent).FullName!,
            typeof(SFA.DAS.Funding.ApprenticeshipEarnings.Types.EarningsGeneratedEvent).FullName!,
            typeof(SFA.DAS.Learning.Types.LearningCreatedEvent).FullName!,
            "SFA.DAS.Payments.FundingSource.Messages.Commands.CalculateOnProgrammePayment", 
            typeof(SFA.DAS.Funding.ApprenticeshipEarnings.Types.ApprenticeshipEarningsRecalculatedEvent).FullName!
        };

        return events;
    }
}