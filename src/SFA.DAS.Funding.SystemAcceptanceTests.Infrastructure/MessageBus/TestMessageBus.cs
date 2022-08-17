using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.MessageBus
{
    public class TestMessageBus
    {
        private IEndpointInstance _endpointInstance;
        public bool IsRunning { get; private set; }

        public async Task Start(FundingConfig config)
        {
            var endpointConfiguration = new EndpointConfiguration(QueueNames.ApprenticeshipCreated)
                    .UseMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                ;

            if (NotUsingLearningTransport(config))
            {
                var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
                Console.WriteLine("AZURE_TENANT_ID: " + tenantId); // TODO: DELETE

                var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
                Console.WriteLine("AZURE_CLIENT_ID: " + clientId); // TODO: DELETE

                var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
                Console.WriteLine("AZURE_CLIENT_SECRET: " + clientSecret); // TODO: DELETE

                var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
                TokenCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                transport.CustomTokenCredential(credential);
                transport.ConnectionString(config.SharedServiceBusFqdn);
                transport.Transactions(TransportTransactionMode.ReceiveOnly);
                transport.SubscriptionRuleNamingConvention(RuleNameShortener.Shorten);
            }
            else
            {
                endpointConfiguration
                    .UseTransport<LearningTransport>()
                    .Transactions(TransportTransactionMode.ReceiveOnly)
                    .StorageDirectory(config.LearningTransportStorageDirectory);
                endpointConfiguration.UseLearningTransport(rs => rs.AddRouting());
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

        public Task Publish(object message)
        {
            return _endpointInstance.Publish(message);
        }

        public Task Send(object message)
        {
            return _endpointInstance.Send(message);
        }

    }
}
