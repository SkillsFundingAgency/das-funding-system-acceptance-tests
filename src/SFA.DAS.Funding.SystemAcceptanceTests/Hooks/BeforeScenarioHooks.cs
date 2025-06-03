using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

[Binding]
public class BeforeScenarioHooks
{
    private static ApprenticeshipsClient? _apprenticeshipsClient;
    private static EarningsOuterClient? _earningsOuterClient;
    private static PaymentsFunctionsClient? _paymentsFunctionsClient;
    private static ApprenticeshipsSqlClient? _apprenticeshipsSqlClient;
    private static EarningsSqlClient? _earningsSqlClient;
    private static PaymentsSqlClient? _paymentsSqlClient;
    private static ApprenticeshipsInnerApiHelper? _apprenticeshipsInnerApiHelper;
    private static EarningsInnerApiHelper? _earningsInnerApiHelper;
    private static bool _clientsAssigned = false;
    private static readonly object _lock = new();

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
        var testData = new TestData(TestUlnProvider.GetNext());
        testData.CurrentCollectionYear = TableExtensions.CalculateAcademicYear("0");
        testData.CurrentCollectionPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];
        context.Set(testData);
    }

    private static void RegisterDependencies(ScenarioContext context)
    {
        if (!_clientsAssigned)
        {
            lock (_lock)
            {
                if (!_clientsAssigned)
                {
                    // The status of clientsAssigned is checked again after acquiring the lock to ensure thread safety.
                    _apprenticeshipsClient = new ApprenticeshipsClient();
                    _earningsOuterClient = new EarningsOuterClient();
                    _paymentsFunctionsClient = new PaymentsFunctionsClient();
                    _apprenticeshipsSqlClient = new ApprenticeshipsSqlClient();
                    _earningsSqlClient = new EarningsSqlClient();
                    _paymentsSqlClient = new PaymentsSqlClient();
                    _apprenticeshipsInnerApiHelper = new ApprenticeshipsInnerApiHelper();
                    _earningsInnerApiHelper = new EarningsInnerApiHelper();

                    _clientsAssigned = true;
                }
            }
        }


        var container = context.ScenarioContainer;
        container.RegisterInstanceAs(_apprenticeshipsClient);
        container.RegisterInstanceAs(_earningsOuterClient);
        container.RegisterInstanceAs(_paymentsFunctionsClient);
        container.RegisterInstanceAs(_apprenticeshipsSqlClient);
        container.RegisterInstanceAs(_earningsSqlClient);
        container.RegisterInstanceAs(_paymentsSqlClient);
        container.RegisterInstanceAs(_apprenticeshipsInnerApiHelper);
        container.RegisterInstanceAs(_earningsInnerApiHelper);
    }
}