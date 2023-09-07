using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks
{
    [Binding]
    public class ServiceBusEndpointHook
    {
        [BeforeTestRun]
        public static async Task SetUpSubscription()
        {
            var config = Configurator.GetConfiguration();
            var queueName = config.FundingSystemAcceptanceTestQueue;
            var azureClient = new AzureServiceBusClient(config.SharedServiceBusFqdn);
            await azureClient.CreateQueueAsync(queueName);
            await azureClient.CreateSubscriptionWithFiltersAsync(
                config.FundingSystemAcceptanceTestSubscription, 
                config.SharedServiceBusTopicEndpoint, queueName, 
                config.EventTypes.Split(", ").ToList());
        }
        
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
        public static void StopEndpoint(ScenarioContext context)
        {
           var dasTestMessageBus = context.Get<TestMessageBus>(TestMessageBusKeys.Das);
           dasTestMessageBus?.Stop();

           var pv2TestMessageBus = context.Get<TestMessageBus>(TestMessageBusKeys.Pv2);
           pv2TestMessageBus?.Stop();
        }

        [AfterTestRun]
        public static async Task TearDownSubscription()
        {
            var config = Configurator.GetConfiguration();
            var queueName = config.FundingSystemAcceptanceTestQueue;
            var azureClient = new AzureServiceBusClient(config.SharedServiceBusFqdn);
            await azureClient.DeleteQueueAsync(queueName);
            await azureClient.DeleteSubscriptionAsync(config.FundingSystemAcceptanceTestSubscription, config.SharedServiceBusTopicEndpoint, queueName);
        }
    }
}
