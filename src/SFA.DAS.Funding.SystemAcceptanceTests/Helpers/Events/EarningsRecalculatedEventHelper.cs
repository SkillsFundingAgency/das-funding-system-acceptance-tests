namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class EarningsRecalculatedEventHelper
{
    internal static async Task ReceiveEarningsRecalculatedEvent(this ScenarioContext context, Guid apprenticeshipKey)
    {
        var testData = context.Get<TestData>();

        await WaitHelper.WaitForIt(() =>
        {
            ApprenticeshipEarningsRecalculatedEvent? earningsRecalculatedEvent =
                ApprenticeshipEarningsRecalculatedEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.ApprenticeshipKey == apprenticeshipKey).message;

            if (earningsRecalculatedEvent != null)
            {
                testData.ApprenticeshipEarningsRecalculatedEvent = earningsRecalculatedEvent;
                return true;
            }
            return false;
        }, "Failed to find published Apprenticeship Earnings Recalculated Event");
    }
}
