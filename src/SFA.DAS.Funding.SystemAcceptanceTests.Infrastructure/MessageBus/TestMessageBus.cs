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

        public async Task Start(FundingConfig config, string endpointName, string connectionString)
        {
            _config = config;
            var endpointConfiguration = new EndpointConfiguration(endpointName)
                    .UseMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                ;

            if (NotUsingLearningTransport(config))
            {
                endpointConfiguration
                    .UseAzureServiceBusTransport(connectionString);
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

        private static bool NotUsingLearningTransport(FundingConfig config)
        {
            return !config.SharedServiceBusFqdn.Contains("Learning");
        }

        public async Task Stop()
        {
            await _endpointInstance.Stop();
            IsRunning = false;
        }

        public Task SendApprenticeshipApprovedMessage(object message) => Send(message, _config.ApprovalsEventHandlersQueue);
        public Task SendReleasePaymentsMessage(object message) => Send(message, _config.ReleasePaymentsEventHandlersQueue);

        public Task Send(object message, string queueName) 
        {
            var options = new SendOptions();
            options.DoNotEnforceBestPractices();
            options.SetDestination(queueName);
            return _endpointInstance.Send(message, options);
        }
    }
}