using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.MessageBus
{
    public class TestMessageBus
    {
        private IEndpointInstance _endpointInstance;
        public bool IsRunning { get; private set; }
        public FundingConfig _config { get; set; }

        public async Task Start(FundingConfig config)
        {
            _config = config;
            var endpointConfiguration = new EndpointConfiguration(config.FundingSystemAcceptanceTestQueue)
                    .UseMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                ;

            if (NotUsingLearningTransport(config))
            {
                endpointConfiguration
                    .UseAzureServiceBusTransport(config.SharedServiceBusFqdn);

                await EnsureQueue(config.SharedServiceBusFqdn, config.FundingSystemAcceptanceTestQueue);
            }
            else
            {
                endpointConfiguration
                    .UseTransport<LearningTransport>()
                    .Transactions(TransportTransactionMode.ReceiveOnly)
                    .StorageDirectory(config.LearningTransportStorageDirectory);
                endpointConfiguration.UseLearningTransport();
            }

            _endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            IsRunning = true;
        }

        private async Task EnsureQueue(string connectionString, string queuePath)
        {
            var manageClient = new ManagementClient(connectionString);

            if (await manageClient.QueueExistsAsync(queuePath))
                return;

            var queueDescription = new QueueDescription(queuePath)
            {
                Path = queuePath,
                DefaultMessageTimeToLive = TimeSpan.FromHours(4) //test messages only, no need to live beyond a generous 4 hours, will keep queues from filling up
            };

            await manageClient.CreateQueueAsync(queueDescription);
        }

        private static bool NotUsingLearningTransport(FundingConfig config)
        {
            return !config.SharedServiceBusFqdn.Contains("Learning");
        }

        public async Task Stop()
        {
            await _endpointInstance.Stop();
            IsRunning = false;
        }

        public Task Send(object message)
        {
            var options = new SendOptions();
            options.DoNotEnforceBestPractices();
            options.SetDestination(_config.ApprovalsEventHandlersQueue);
            return _endpointInstance.Send(message, options);
        }
    }
}