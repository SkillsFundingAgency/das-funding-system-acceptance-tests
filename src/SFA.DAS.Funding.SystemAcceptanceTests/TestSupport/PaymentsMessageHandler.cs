using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class PaymentsMessageHandler
    {
        private readonly ScenarioContext _context;

        public PaymentsMessageHandler(ScenarioContext context)
        {
            _context = context;
        }

        public async Task ReceivePaymentsEvent(Guid apprenticeshipKey)
        {
            await WaitHelper.WaitForIt(() =>
            {
                PaymentsGeneratedEvent? paymentsEvent =
                    PaymentsGeneratedEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.ApprenticeshipKey == apprenticeshipKey).message;

                if (paymentsEvent != null)
                {
                    _context.Set(paymentsEvent);
                    return true;
                }
                return false;
            }, "Failed to find published event in Payments");
        }

#pragma warning disable CA1822 // Mark members as static
        public async Task PublishReleasePaymentsCommand(ReleasePaymentsCommand releasePaymentsCommand)
        {
            await TestServiceBus.Das.SendReleasePaymentsMessage(releasePaymentsCommand);
        }
#pragma warning restore CA1822 // Mark members as static
    }
}
