using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure;
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
                EventList.GetEventTypes());
        }
        
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
        public static void StopEndpoint(ScenarioContext context)
        {
            TestServiceBus.Das?.Stop();
            TestServiceBus.Pv2?.Stop();
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
