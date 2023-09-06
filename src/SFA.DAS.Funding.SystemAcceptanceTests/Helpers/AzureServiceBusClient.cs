using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    internal class AzureServiceBusClient
    {
        private readonly ServiceBusAdministrationClient _administrationClient;
        private static string TopicSubscriptionFilterName => "AllEvents";

        public AzureServiceBusClient(string azureServiceBusNamespace)
        {
            _administrationClient = new ServiceBusAdministrationClient(azureServiceBusNamespace, new DefaultAzureCredential());
        }

        internal async Task CreateSubscriptionWithFiltersAsync(string subscriptionName, string topicName,
            string destinationQueueName, List<string>? filterEventTypes)
        {
            await CreateSubscriptionAsync(subscriptionName, topicName, destinationQueueName);

            if (filterEventTypes is { Count: > 0 })
            {
                await DeleteDefaultRuleAsync(subscriptionName, topicName);
                await CreateNewSqlFilter(subscriptionName, topicName, filterEventTypes);
            }
        }

        private async Task CreateNewSqlFilter(string subscriptionName, string topicName, List<string> filterEventTypes)
        {
            try
            {
                var sqlExpression = "[NServiceBus.EnclosedMessageTypes] LIKE '%" +
                                    string.Join("%' OR [NServiceBus.EnclosedMessageTypes] LIKE '%",
                                        filterEventTypes) + "%'";
                await _administrationClient.CreateRuleAsync(topicName, subscriptionName, new CreateRuleOptions
                {
                    Name = TopicSubscriptionFilterName,
                    Filter = new SqlRuleFilter(sqlExpression)
                });
            }
            catch (ServiceBusException serviceBusException)
            {
                // Do not fail if subscription filter already exists
                if (serviceBusException.Reason != ServiceBusFailureReason.MessagingEntityAlreadyExists)
                {
                    Assert.Fail(
                        $"Attempted to create filter with name {TopicSubscriptionFilterName} for {subscriptionName} for topic {topicName} but unsuccessful due to: '{serviceBusException.Reason}' exception from Azure ServiceBus. Time: {DateTime.Now:G}.");
                }
            }
        }

        private async Task DeleteDefaultRuleAsync(string subscriptionName, string topicName)
        {
            try
            {
                await _administrationClient.DeleteRuleAsync(topicName, subscriptionName,
                    CreateRuleOptions.DefaultRuleName);
            }
            catch (ServiceBusException serviceBusException)
            {
                // Do not fail if default rule doesn't exist
                if (serviceBusException.Reason != ServiceBusFailureReason.MessagingEntityNotFound)
                {
                    Assert.Fail(
                        $"Attempted to delete the default filter rule on subscription {subscriptionName} for topic {topicName} but unsuccessful due to: '{serviceBusException.Reason}' exception from Azure ServiceBus. Time: {DateTime.Now:G}.");
                }
            }
        }

        private async Task CreateSubscriptionAsync(string subscriptionName, string topicName, string destinationQueueName)
        {
            try
            {
                var subscriptionsOptions = new CreateSubscriptionOptions(topicName, subscriptionName)
                    { ForwardTo = destinationQueueName };
                await _administrationClient.CreateSubscriptionAsync(subscriptionsOptions);
            }
            catch (ServiceBusException serviceBusException)
            {
                // Do not fail if subscription already exists
                if (serviceBusException.Reason != ServiceBusFailureReason.MessagingEntityAlreadyExists)
                {
                    Assert.Fail(
                        $"Attempted to create subscription with name {subscriptionName} for topic {topicName} but unsuccessful due to: '{serviceBusException.Reason}' exception from Azure ServiceBus. Time: {DateTime.Now:G}.");
                }
            }
        }

        internal async Task CreateQueueAsync(string queueName)
        {
            try
            {
                await _administrationClient.CreateQueueAsync(
                    new CreateQueueOptions(queueName)
                    {
                        DefaultMessageTimeToLive =
                            TimeSpan.FromHours(
                                4) // Prevents queue from filling up if it doesn't get deleted successfully after test run
                    });
            }
            catch (ServiceBusException serviceBusException)
            {
                // Don't fail if queue already exists
                if (serviceBusException.Reason != ServiceBusFailureReason.MessagingEntityAlreadyExists)
                {
                    Assert.Fail($"Attempted to create queue with name {queueName} but unsuccessful due to: '{serviceBusException.Reason}' exception from Azure ServiceBus. Time: {DateTime.Now:G}.");
                }
            }
        }
        internal async Task DeleteSubscriptionAsync(string subscriptionName, string topicName, string destinationQueueName)
        {
            try
            {
                await _administrationClient.DeleteSubscriptionAsync(topicName, subscriptionName);
            }
            catch (ServiceBusException serviceBusException)
            {
                // Don't fail if subscription not found
                if (serviceBusException.Reason != ServiceBusFailureReason.MessagingEntityNotFound)
                {
                    Assert.Fail($"Attempted to delete subscription with name {subscriptionName} for topic {topicName} but unsuccessful due to: '{serviceBusException.Reason}' exception from Azure ServiceBus. Time: {DateTime.Now:G}.");
                }
            }
        }

        internal async Task DeleteQueueAsync(string queueName)
        {
            try
            {
                await _administrationClient.DeleteQueueAsync(queueName);
            }
            catch (ServiceBusException serviceBusException)
            {
                // Don't fail if queue not found
                if (serviceBusException.Reason != ServiceBusFailureReason.MessagingEntityNotFound)
                {
                    Assert.Fail($"Attempted to delete queue with name {queueName} but unsuccessful due to: '{serviceBusException.Reason}' exception from Azure ServiceBus. Time: {DateTime.Now:G}.");
                }
            }
        }
    }
}
