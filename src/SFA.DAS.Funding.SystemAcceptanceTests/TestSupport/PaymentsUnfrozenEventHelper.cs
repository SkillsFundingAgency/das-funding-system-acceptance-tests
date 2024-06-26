using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class PaymentsUnfrozenEventHelper
    {
        private readonly ScenarioContext _context;
        public PaymentsUnfrozenEventHelper(ScenarioContext context)
        {
            _context = context;
        }

        public PaymentsUnfrozenEvent CreatePaymentsUnfrozenMessage()
        {
            var apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();

            var fixture = new Fixture();
            return fixture.Build<PaymentsUnfrozenEvent>()
                .With(_ => _.ApprenticeshipKey, apprenticeshipCreatedEvent.ApprenticeshipKey)
                .Create();
        }

        public async Task PublishPaymentsUnfrozenEvent(PaymentsUnfrozenEvent paymentsUnfrozenEvent)
        {
            await TestServiceBus.Das.SendPaymentsUnfrozenMessage(paymentsUnfrozenEvent);
        }
    }
}
