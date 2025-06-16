using Azure.Messaging.ServiceBus;
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
        private FundingConfig Config { get; set; } = null!;

        public async Task Start(FundingConfig config, string endpointName)
        {
            Config = config;
            var endpointConfiguration = new EndpointConfiguration(endpointName)
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer();



            if (NotUsingLearningTransport(config))
            {
                endpointConfiguration
                    .UseAzureServiceBusTransport(Config.SharedServiceBusFqdn);
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

        public async Task SendApprenticeshipApprovedMessage(object message) => await Send(message, Config.ApprovalCreatedQueue);
        public async Task SendPriceChangeApprovedMessage(object message) => await Send(message, Config.PriceChangeApprovedEventHandlersQueue);
        public async Task SendStartDateChangedMessage(object message) => await Send(message, Config.StartDateChangeApprovedEventHandlersQueue);
        public async Task SendPaymentsFrozenMessage(object message) => await Send(message, Config.PaymentsFrozenEventHandlersQueue);
        public async Task SendPaymentsUnfrozenMessage(object message) => await Send(message, Config.PaymentsUnfrozenEventHandlersQueue);


        public async Task Send(object message, string queueName)
        {
            var options = new SendOptions();
            options.DoNotEnforceBestPractices();
            options.SetDestination(queueName);
            await _endpointInstance.Send(message, options);
        }
    }
}