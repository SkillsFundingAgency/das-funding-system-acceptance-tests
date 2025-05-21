using SFA.DAS.Funding.ApprenticeshipPayments.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;


internal static class PaymentsEventHelper
{
    internal static async Task ReceivePaymentsEvent(this ScenarioContext context, Guid apprenticeshipKey)
    {
        await WaitHelper.WaitForIt(() =>
        {
            PaymentsGeneratedEvent? paymentsEvent =
                PaymentsGeneratedEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.ApprenticeshipKey == apprenticeshipKey).message;

            if (paymentsEvent != null)
            {
                context.Set(paymentsEvent);
                return true;
            }
            return false;
        }, "Failed to find published event in Payments");
    }
}
