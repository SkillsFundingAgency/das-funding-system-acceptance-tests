using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    internal class AzureServiceBusClient
    {
        private readonly ServiceBusAdministrationClient _administrationClient;

        public AzureServiceBusClient(string azureServiceBusNamespace)
        {
            _administrationClient = new ServiceBusAdministrationClient(azureServiceBusNamespace, new DefaultAzureCredential());
        }

        internal async Task CreateSubscriptionAsync(string subscriptionName, string topicName, string destinationQueueName, List<string>? filterEventTypes = null)
        {
            var subscriptionExists = await _administrationClient.SubscriptionExistsAsync(topicName, subscriptionName);
            if (!subscriptionExists)
            {
                // Create subscription
                var subscriptionsOptions = new CreateSubscriptionOptions(topicName, subscriptionName) { ForwardTo = destinationQueueName };
                await _administrationClient.CreateSubscriptionAsync(subscriptionsOptions);

                if (filterEventTypes != null && filterEventTypes.Count > 0)
                {
                    // Remove any default rules
                    await _administrationClient.DeleteRuleAsync(topicName, subscriptionName, CreateRuleOptions.DefaultRuleName);

                    // Create filters
                    var sqlExpression = "[NServiceBus.EnclosedMessageTypes] LIKE '%" + string.Join("%' OR [NServiceBus.EnclosedMessageTypes] LIKE '%", filterEventTypes);
                    await _administrationClient.CreateRuleAsync(topicName, subscriptionName, new CreateRuleOptions
                    {
                        Name = $"{new Guid()}",
                        Filter = new SqlRuleFilter(sqlExpression)});
                }
            }
            else
            {
                Assert.Fail(
                    $"Attempted to create subscription with name {subscriptionName} but already exists. Time: {DateTime.Now:G}.");
            }
        }

        internal async Task CreateQueueAsync(string queueName)
        {
            var queueExists = await _administrationClient.QueueExistsAsync(queueName);
            if (!queueExists)
            {
                await _administrationClient.CreateQueueAsync(new CreateQueueOptions(queueName)
                {
                    DefaultMessageTimeToLive = TimeSpan.FromHours(4) // Prevents queue from filling up if it doesn't get deleted successfully after test run
                });
            }
            else
            {
                Assert.Fail($"Attempted to create queue with name {queueName} but already exists. Time: {DateTime.Now:G}.");
            }

            //var data = new object()
            //{

            //};
            //var stringContent = data != null
            //    ? new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json")
            //    : null;
            
            //var apiVersion = _config.ApiVersion;
            //var subscriptionId = _config.SubscriptionId;
            //var resourceGroupName = _config.ServiceBusResourceGroupName;
            //var resourceProviderNamespace = "Microsoft.ServiceBus";
            //var parentResourcePath = "";
            //var resourceType = "";
            //var resourceName = "";

            //var url =
            //    $"https://management.azure.com/subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}/providers/{resourceProviderNamespace}/{parentResourcePath}/{resourceType}/{resourceName}?api-version={apiVersion}";

            //var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            //requestMessage.Content = stringContent;

            //var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            //var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            //response.EnsureSuccessStatusCode();
        }
        internal async Task DeleteSubscriptionAsync(string subscriptionName, string topicName, string destinationQueueName)
        {
            var subscriptionExists = await _administrationClient.SubscriptionExistsAsync(topicName, subscriptionName);
            if (subscriptionExists)
            {
                await _administrationClient.DeleteSubscriptionAsync(topicName, subscriptionName);
            }
            else
            {
                Assert.Fail(
                    $"Attempted to delete subscription with name {subscriptionName} but does not exist. Time: {DateTime.Now:G}.");
            }
        }

        internal async Task DeleteQueueAsync(string queueName)
        {
            var queueExists = await _administrationClient.QueueExistsAsync(queueName);
            if (queueExists)
            {
                await _administrationClient.DeleteQueueAsync(queueName);
            }
            else
            {
                Assert.Fail($"Attempted to delete queue with name {queueName} but does not exist. Time: {DateTime.Now:G}.");
            }
        }
    }
}
