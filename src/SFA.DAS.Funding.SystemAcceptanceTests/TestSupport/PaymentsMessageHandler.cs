using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class PaymentsMessageHandler
    {
        private readonly ScenarioContext _context;
        private const int SecondToWaitBeforePublishingReleasePaymentsCommandToSimulateEndOfMonth = 10;

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

        public async Task PublishReleasePaymentsCommand(ReleasePaymentsCommand releasePaymentsCommand)
        {
            Thread.Sleep(TimeSpan.FromSeconds(SecondToWaitBeforePublishingReleasePaymentsCommandToSimulateEndOfMonth));
            await _context.Get<TestMessageBus>(TestMessageBusKeys.Das).SendReleasePaymentsMessage(releasePaymentsCommand);
        }
    }
}
