using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class PaymentsFrozenEventHelper
    {
        private readonly ScenarioContext _context;
        public PaymentsFrozenEventHelper(ScenarioContext context)
        {
            _context = context;
        }

        public PaymentsFrozenEvent CreatePaymentsFrozenMessage()
        {
            var apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();

            var fixture = new Fixture();
            return fixture.Build<PaymentsFrozenEvent>()
                .With(_ => _.ApprenticeshipKey, apprenticeshipCreatedEvent.ApprenticeshipKey)
                .Create();
        }

        public async Task PublishPaymentsFrozenEvent(PaymentsFrozenEvent paymentsFrozenEvent)
        {
            await TestServiceBus.Das.SendPaymentsFrozenMessage(paymentsFrozenEvent);
        }
    }
}
