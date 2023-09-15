namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks
{
    [Binding]
    public class ServiceBusEndpointHook
    {
        [BeforeTestRun(Order = 1)]
        public static void StartEndpoint()
        {
            var config = Configurator.GetConfiguration();
            TestServiceBus.Config = config;

            var dasTestMessageBus = new TestMessageBus();
            dasTestMessageBus.Start(config, config.FundingSystemAcceptanceTestQueue, config.SharedServiceBusFqdn).GetAwaiter().GetResult();
            TestServiceBus.Das = dasTestMessageBus;

            var pv2TestMessageBus = new TestMessageBus();
            pv2TestMessageBus.Start(config, config.Pv2FundingSourceQueue, config.Pv2ServiceBusFqdn).GetAwaiter().GetResult();
            TestServiceBus.Pv2 = pv2TestMessageBus;
        }

        [AfterTestRun(Order = 1)]
        public static void StopEndpoint()
        {
            TestServiceBus.Das?.Stop();
            TestServiceBus.Pv2?.Stop();
        }
    }
}
