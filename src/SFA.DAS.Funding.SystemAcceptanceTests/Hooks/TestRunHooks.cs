using System.Reflection;
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
    private readonly ScenarioContext _context;


    public TestRunHooks (ScenarioContext context)
    {
        _context = context;
    }

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
        var ulns = TestUlnProvider.Initialise(GetNumberOfLearnersRequired());// automatically generate the number of ULNs needed for a test run
        var testLearners = ulns.Select(uln => DcLearnerDataHelper.GetLearner(uln)).ToList();
        var wireMockClient = new WireMockClient();
        var currentAcademicYear = Convert.ToInt32(TableExtensions.CalculateAcademicYear("CurrentMonth+0"));
        await wireMockClient.CreateMockResponse($"learners/{currentAcademicYear}?ukprn={Constants.UkPrn}&fundModel=36&progType=-1&standardCode=-1&pageNumber=1&pageSize=1000", testLearners);
    }

    [BeforeTestRun(Order = 5)]
    public static void RegisterSingletons()
    {
        StaticObjects.EarningsOuterClient = new EarningsOuterClient();
        StaticObjects.ApprenticeshipsSqlClient = new LearningSqlClient();
        StaticObjects.EarningsSqlClient = new EarningsSqlClient();
        StaticObjects.ApprenticeshipsInnerApiHelper = new ApprenticeshipsInnerApiHelper();
        StaticObjects.EarningsInnerApiHelper = new EarningsInnerApiHelper();
        StaticObjects.LearnerDataOuterApiHelper = new LearnerDataOuterApiHelper();
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

    [AfterTestRun(Order = 4)]
    public static void StopEndpoints()
    {
        TestServiceBus.Das?.Stop();
        // TestServiceBus.Pv2?.Stop();
    }

    private static int GetNumberOfLearnersRequired()
    {
        var methods = Assembly.GetExecutingAssembly().GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

        var testCount = 0;

        foreach (var method in methods)
        {
            var testCases = method.GetCustomAttributes(typeof(TestCaseAttribute), inherit: true)
                .Cast<TestCaseAttribute>()
                .ToList();

            var hasPagingAttribute = method
                .GetCustomAttributes(typeof(NUnit.Framework.CategoryAttribute), inherit: true)
                .Cast<NUnit.Framework.CategoryAttribute>()
                .Any(x=>x.Name == "Creates15RecordsForPaging");

            if (testCases.Any())
            {
                testCount += testCases.Count;
                if(hasPagingAttribute)
                    testCount += testCases.Count * 14; // each test case will need 15 records, so add 14 more per test case

            }
            else if (method.GetCustomAttributes(typeof(TestAttribute), inherit: true).Any())
            {
                testCount++;
                if (hasPagingAttribute)
                    testCount += 14; // each test will need 15 records, so add 14 more
            }

        }
        return testCount;
    }

}
