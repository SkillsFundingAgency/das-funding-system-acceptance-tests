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
        if(_apprenticeshipsClient == null)
            _apprenticeshipsClient = new ApprenticeshipsClient();

        if (_earningsOuterClient == null)
            _earningsOuterClient = new EarningsOuterClient();

        if (_paymentsFunctionsClient == null)
            _paymentsFunctionsClient = new PaymentsFunctionsClient();

        if (_apprenticeshipsSqlClient == null)
            _apprenticeshipsSqlClient = new ApprenticeshipsSqlClient();

        if (_earningsSqlClient == null)
            _earningsSqlClient = new EarningsSqlClient();

        if (_paymentsSqlClient == null)
            _paymentsSqlClient = new PaymentsSqlClient();

        if(_apprenticeshipsInnerApiHelper == null)
            _apprenticeshipsInnerApiHelper = new ApprenticeshipsInnerApiHelper();

        if (_earningsInnerApiHelper == null)
            _earningsInnerApiHelper = new EarningsInnerApiHelper();

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