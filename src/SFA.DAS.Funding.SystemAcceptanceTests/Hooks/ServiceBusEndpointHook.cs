using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks
{
    [Binding]
    public class ServiceBusEndpointHook
    {
        [BeforeScenario(Order = 2)]
        public static async Task StartEndpoint(ScenarioContext context)
        {
            var config = context.Get<FundingConfig>();

            var dasTestMessageBus = new TestMessageBus();
            await dasTestMessageBus.Start(config, config.FundingSystemAcceptanceTestQueue, config.SharedServiceBusFqdn);
            context.Set(dasTestMessageBus, TestMessageBusKeys.Das);

            var pv2TestMessageBus = new TestMessageBus();
            await pv2TestMessageBus.Start(config, config.Pv2FundingSourceQueue, config.Pv2ServiceBusFqdn);
            context.Set(pv2TestMessageBus, TestMessageBusKeys.Pv2);
        }

        [AfterScenario(Order = 1)]
        public void StopEndpoint(ScenarioContext context)
        {
           var dasTestMessageBus = context.Get<TestMessageBus>(TestMessageBusKeys.Das);
           dasTestMessageBus?.Stop();

           var pv2TestMessageBus = context.Get<TestMessageBus>(TestMessageBusKeys.Pv2);
           pv2TestMessageBus?.Stop();
        }
    }
}
