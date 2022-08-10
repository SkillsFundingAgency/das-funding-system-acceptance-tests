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

            var testMessageBus = new TestMessageBus();
            await testMessageBus.Start(config);
            context.Set(testMessageBus);
        }

        [AfterScenario(Order = 1)]
        public void StopEndpoint(ScenarioContext context)
        {
           var testMessageBus = context.Get<TestMessageBus>();
           testMessageBus?.Stop();
        }
    }
}
