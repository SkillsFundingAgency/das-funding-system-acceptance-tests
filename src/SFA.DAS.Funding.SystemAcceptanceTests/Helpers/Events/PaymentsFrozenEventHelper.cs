using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class PaymentsFrozenEventHelper
{

    internal static PaymentsFrozenEvent CreatePaymentsFrozenMessage(this ScenarioContext context)
    {
        var testData = context.Get<TestData>();

        var fixture = new Fixture();
        return fixture.Build<PaymentsFrozenEvent>()
            .With(_ => _.LearningKey, testData.LearningKey)
            .Create();
    }

    internal static async Task PublishPaymentsFrozenEvent(PaymentsFrozenEvent paymentsFrozenEvent)
    {
        await TestServiceBus.Das.SendPaymentsFrozenMessage(paymentsFrozenEvent);
    }
}
