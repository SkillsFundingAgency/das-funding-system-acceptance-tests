using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class PaymentsMessageHandler
    {
        private readonly ScenarioContext _context;
        private readonly FundingConfig _fundingConfig;

        public PaymentsMessageHandler(ScenarioContext context)
        {
            _context = context;
            _fundingConfig = Configurator.GetConfiguration();
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
    }
}
