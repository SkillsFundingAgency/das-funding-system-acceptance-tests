﻿using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class PaymentsUnfrozenEventHelper
{

    internal static PaymentsUnfrozenEvent CreatePaymentsUnfrozenMessage(this ScenarioContext context)
    {
        var testData = context.Get<TestData>();

        var fixture = new Fixture();
        return fixture.Build<PaymentsUnfrozenEvent>()
            .With(_ => _.LearningKey, testData.LearningKey)
            .Create();
    }

    internal static async Task PublishPaymentsUnfrozenEvent(PaymentsUnfrozenEvent paymentsUnfrozenEvent)
    {
        await TestServiceBus.Das.SendPaymentsUnfrozenMessage(paymentsUnfrozenEvent);
    }
}
