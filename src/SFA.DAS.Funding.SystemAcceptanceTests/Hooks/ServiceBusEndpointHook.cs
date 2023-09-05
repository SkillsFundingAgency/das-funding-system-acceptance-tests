using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks
{
    [Binding]
    public class ServiceBusEndpointHook
    {
        private readonly string _queueName;
        private readonly string _subscriptionName;
        private readonly string _topicEndpointName;
        private readonly string _sharedServiceBusFqdn;
        private readonly List<string> _eventTypes;
        public ServiceBusEndpointHook()
        {
            var config = Configurator.GetConfiguration();
            _queueName = config.FundingSystemAcceptanceTestQueue;
            _sharedServiceBusFqdn = config.SharedServiceBusFqdn;
            _topicEndpointName = config.SharedServiceBusTopicEndpoint;
            _subscriptionName = config.FundingSystemAcceptanceTestSubscription;
            _eventTypes = config.EventTypes.Split(", ").ToList();
        }

        [BeforeTestRun]
        public async Task SetUpSubscription()
        {
            var azureClient = new AzureServiceBusClient(_sharedServiceBusFqdn);
            await azureClient.CreateQueueAsync(_queueName);
            await azureClient.CreateSubscriptionAsync(_subscriptionName, _topicEndpointName, _queueName, _eventTypes);
        }
        
        [BeforeScenario(Order = 2)]
        public async Task StartEndpoint(ScenarioContext context)
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

        [AfterTestRun]
        public async Task TearDownSubscription()
        {
            var azureClient = new AzureServiceBusClient(_sharedServiceBusFqdn);
            await azureClient.DeleteQueueAsync(_queueName);
            await azureClient.DeleteSubscriptionAsync(_subscriptionName, _topicEndpointName, _queueName);
        }
    }
}
