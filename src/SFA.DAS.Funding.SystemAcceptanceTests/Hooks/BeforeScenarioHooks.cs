using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using TechTalk.SpecFlow;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

[Binding]
public class BeforeScenarioHooks
{
    private static PaymentsFunctionsClient? _paymentsFunctionsClient;
    private static PaymentsSqlClient? _paymentsSqlClient;
    private static EarningsSqlClient? _earningsSqlClient;
    private static EarningsOuterClient? _earningsOuterClient;

    [BeforeScenario(Order = 1)]
    public void BeforeScenarioHook(ScenarioContext context)
    {
        var config = Configurator.GetConfiguration();
        context.Set(config);
        Console.WriteLine($"Begin Scenario {context.ScenarioInfo.Title}");


        if(context.ScenarioInfo.Tags.Contains("releasesPayments"))
        {
            if (config.ShouldReleasePayments == false)
            {
                Assert.Ignore("Skipping scenario: Release payments is disabled in test environment variables.");
            }
        }

        PopulateContextTestData(context);
        RegisterDependencies(context);
    }

    private static void PopulateContextTestData(ScenarioContext context)
    {
        var testData = new TestData();
        testData.CurrentCollectionYear = TableExtensions.CalculateAcademicYear("0");
        testData.CurrentCollectionPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];
        context.Set(testData);
    }

    private static void RegisterDependencies(ScenarioContext context)
    {
        if (_paymentsFunctionsClient == null)
        {
            _paymentsFunctionsClient = new PaymentsFunctionsClient();
        }

        if (_paymentsSqlClient == null)
        {
            _paymentsSqlClient = new PaymentsSqlClient();
        }

        if (_earningsSqlClient == null)
        {
            _earningsSqlClient = new EarningsSqlClient();
        }

        if (_earningsOuterClient == null)
        {
            _earningsOuterClient = new EarningsOuterClient();
        }

        var container = context.ScenarioContainer;
        container.RegisterInstanceAs(_paymentsFunctionsClient);
        container.RegisterInstanceAs(_paymentsSqlClient);
        container.RegisterInstanceAs(_earningsSqlClient);
        container.RegisterInstanceAs(_earningsOuterClient);
    }
}