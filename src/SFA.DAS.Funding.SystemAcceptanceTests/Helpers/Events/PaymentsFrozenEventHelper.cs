using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class PaymentsFrozenEventHelper
{

    internal static PaymentsFrozenEvent CreatePaymentsFrozenMessage(this ScenarioContext context)
    {
        var apprenticeshipCreatedEvent = context.Get<ApprenticeshipCreatedEvent>();

        var fixture = new Fixture();
        return fixture.Build<PaymentsFrozenEvent>()
            .With(_ => _.ApprenticeshipKey, apprenticeshipCreatedEvent.ApprenticeshipKey)
            .Create();
    }

    internal static async Task PublishPaymentsFrozenEvent(PaymentsFrozenEvent paymentsFrozenEvent)
    {
        await TestServiceBus.Das.SendPaymentsFrozenMessage(paymentsFrozenEvent);
    }
}
