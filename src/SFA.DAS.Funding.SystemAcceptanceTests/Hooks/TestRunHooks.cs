using Azure.Identity;
using Azure.Messaging.ServiceBus;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using WireMock.Server;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

[Binding]
public class TestRunHooks
{


    [BeforeTestRun(Order = 1)]
    public static void StartLocalWireMockServer()
    {
        if (Configurator.EnvironmentName == "LOCAL")
        {
            var localWireMockUrl = new Uri(Configurator.GetConfiguration().WireMockBaseUrl);
            if (localWireMockUrl.Host != "localhost")
                throw new ArgumentException("WireMockBaseUrl must be localhost when running tests against local environment.");

            StaticObjects.WireMockServer = WireMockServer.Start(localWireMockUrl.Port);
        }
    }

    [BeforeTestRun(Order = 2)]
    public static async Task SetUpAzureServiceBusSubscription()
    {
        TestServiceBus.Config = Configurator.GetConfiguration();
        var queueName = TestServiceBus.Config.FundingSystemAcceptanceTestQueue;
        var azureClient = new AzureServiceBusClient(TestServiceBus.Config.SharedServiceBusFqdn);
        await azureClient.CreateQueueAsync(queueName);
        await azureClient.CreateSubscriptionWithFiltersAsync(
            TestServiceBus.Config.FundingSystemAcceptanceTestSubscription,
            TestServiceBus.Config.SharedServiceBusTopicEndpoint, queueName, 
            EventList.GetEventTypes());
    }

    [BeforeTestRun(Order = 3)]
    public static void StartEndpoints()
    {
        var config = Configurator.GetConfiguration();
        TestServiceBus.Config = config;

        var dasTestMessageBus = new TestMessageBus();
        dasTestMessageBus.Start(config, config.FundingSystemAcceptanceTestQueue).GetAwaiter().GetResult();
        TestServiceBus.Das = dasTestMessageBus;

        // var pv2TestMessageBus = new TestMessageBus();
        // pv2TestMessageBus.Start(config, config.Pv2FundingSourceQueue, config.Pv2ServiceBusFqdn).GetAwaiter().GetResult();
        // TestServiceBus.Pv2 = pv2TestMessageBus;

        BaseQueueReciever.SetConfig(config);
    }

    [BeforeTestRun(Order = 4)]
    public static async Task GenerateSharedTestData()
    {
        var ulns = TestUlnProvider.Initialise(100);// increase this number as the number of tests increases
        var testLearners = ulns.Select(uln => DcLearnerDataHelper.GetLearner(uln)).ToList();
        var wireMockClient = new WireMockClient();
        var currentAcademicYear = Convert.ToInt32(TableExtensions.CalculateAcademicYear("CurrentMonth+0"));
        await wireMockClient.CreateMockResponse($"learners/{currentAcademicYear}?ukprn={Constants.UkPrn}&fundModel=36&progType=-1&standardCode=-1&pageNumber=1&pageSize=1000", testLearners);
    }

    [BeforeTestRun(Order = 5)]
    public static void RegisterSingletons()
    {
        StaticObjects.ApprenticeshipsClient = new LearningClient();
        StaticObjects.EarningsOuterClient = new EarningsOuterClient();
        StaticObjects.ApprenticeshipsSqlClient = new LearningSqlClient();
        StaticObjects.EarningsSqlClient = new EarningsSqlClient();
        StaticObjects.PaymentsSqlClient = new PaymentsSqlClient();
        StaticObjects.ApprenticeshipsInnerApiHelper = new ApprenticeshipsInnerApiHelper();
        StaticObjects.EarningsInnerApiHelper = new EarningsInnerApiHelper();
    }


    [AfterTestRun(Order = 1)]
    public static void StopLocalWireMockServer()
    {
        StaticObjects.WireMockServer?.Stop();
    }

    [AfterTestRun(Order = 2)]
    public static async Task TearDownSubscription()
    {
        var config = Configurator.GetConfiguration();
        var queueName = config.FundingSystemAcceptanceTestQueue;
        var azureClient = new AzureServiceBusClient(config.SharedServiceBusFqdn);
        await azureClient.DeleteQueueAsync(queueName);
        await azureClient.DeleteSubscriptionAsync(config.FundingSystemAcceptanceTestSubscription, config.SharedServiceBusTopicEndpoint, queueName);
    }

    [AfterTestRun(Order = 3)]
    public static async Task PurgePv2FundingSourceQueue()
    {
        var config = Configurator.GetConfiguration();

        if (config.EnvironmentName != "DEMO")
        {
            await using var client = new ServiceBusClient(config.SharedServiceBusFqdn, new DefaultAzureCredential());
            var receiver = client.CreateReceiver(config.Pv2FundingSourceQueue, new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                SubQueue = SubQueue.DeadLetter
            });

            var totalMessages = 0;

            while (true)
            {
                var messages = await receiver.ReceiveMessagesAsync(maxMessages: 100, maxWaitTime: TimeSpan.FromSeconds(5));
                LoggerHelper.WriteLog($"Purging {messages.Count} dead-letter messages from {config.Pv2FundingSourceQueue}.");
                totalMessages += messages.Count;
                if (messages.Count == 0)
                {
                    LoggerHelper.WriteLog($"All dead-letter messages purged from {config.Pv2FundingSourceQueue}. Total {totalMessages} messages purged.");
                    break;
                }
            }
        }
    }

    [AfterTestRun(Order = 4)]
    public static void StopEndpoints()
    {
        TestServiceBus.Das?.Stop();
        // TestServiceBus.Pv2?.Stop();
    }

}
