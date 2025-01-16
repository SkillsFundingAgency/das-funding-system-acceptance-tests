using NServiceBus;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System.Threading;

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
                .UseNewtonsoftJsonSerializer();



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

        public async Task SendApprenticeshipApprovedMessage(object message) => await Send(message, _config.ApprovalsEventHandlersQueue);
        public async Task SendReleasePaymentsMessage(object message) => await Send(message, _config.ReleasePaymentsEventHandlersQueue);
        public async Task SendPriceChangeApprovedMessage(object message) => await Send(message, _config.PriceChangeApprovedEventHandlersQueue);
        public async Task SendStartDateChangedMessage(object message) => await Send(message, _config.StartDateChangeApprovedEventHandlersQueue);
        public async Task SendPaymentsFrozenMessage(object message) => await Send(message, _config.PaymentsFrozenEventHandlersQueue);
        public async Task SendPaymentsUnfrozenMessage(object message) => await Send(message, _config.PaymentsUnfrozenEventHandlersQueue);


        public async Task Send(object message, string queueName)
        {
            Console.WriteLine($"Sending message to {queueName}");
            var options = new SendOptions();
            options.DoNotEnforceBestPractices();
            options.SetDestination(queueName);
            await _endpointInstance.Send(message, options);
        }
    }
}