using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using System.Linq;

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
                    PaymentsGeneratedEventHandler.ReceivedEvents.FirstOrDefault(x => x.ApprenticeshipKey == apprenticeshipKey);

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
            await _context.Get<TestMessageBus>().SendReleasePaymentsMessage(releasePaymentsCommand);
        }
    }
}
