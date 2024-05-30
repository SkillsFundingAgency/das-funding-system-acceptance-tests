namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public class EarningsRecalculatedEventHelper
{
    private ScenarioContext _context;

    public EarningsRecalculatedEventHelper(ScenarioContext context)
    {
        _context = context;
    }

    public async Task ReceiveEarningsRecalculatedEvent(Guid apprenticeshipKey)
    {
        await WaitHelper.WaitForIt(() =>
        {
            ApprenticeshipEarningsRecalculatedEvent? earningsRecalculatedEvent =
                ApprenticeshipEarningsRecalculatedEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.ApprenticeshipKey == apprenticeshipKey).message;

            if (earningsRecalculatedEvent != null)
            {
                _context.Set(earningsRecalculatedEvent);
                return true;
            }
            return false;
        }, "Failed to find published Apprenticeship Earnings Recalculated Event");
    }
}