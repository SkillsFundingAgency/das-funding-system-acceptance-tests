using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

[Binding]
public class BeforeScenarioHooks
{
    [BeforeScenario(Order = 1)]
    public void BeforeScenarioHook(ScenarioContext context)
    {
        var config = Configurator.GetConfiguration();
        context.Set(config);
        Console.WriteLine($"Begin Scenario {context.ScenarioInfo.Title}");


        if(context.ScenarioInfo.Tags.Contains("releasesPayments") && !config.ShouldReleasePayments)
        {
            Assert.Ignore("Skipping scenario: Release payments is disabled in test environment variables.");
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
        var container = context.ScenarioContainer;
        container.RegisterInstanceAs(StaticObjects.ApprenticeshipsClient);
        container.RegisterInstanceAs(StaticObjects.EarningsOuterClient);
        container.RegisterInstanceAs(StaticObjects.PaymentsFunctionsClient);
        container.RegisterInstanceAs(StaticObjects.ApprenticeshipsSqlClient);
        container.RegisterInstanceAs(StaticObjects.EarningsSqlClient);
        container.RegisterInstanceAs(StaticObjects.PaymentsSqlClient);
        container.RegisterInstanceAs(StaticObjects.ApprenticeshipsInnerApiHelper);
        container.RegisterInstanceAs(StaticObjects.EarningsInnerApiHelper);
    }
}