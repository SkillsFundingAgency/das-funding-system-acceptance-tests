﻿using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

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


        if(context.ScenarioInfo.Tags.Contains("releasesPayments"))
        {
            if (config.ShouldReleasePayments == false)
            {
                Assert.Ignore("Skipping scenario: Release payments is disabled in test environment variables.");
            }
        }

        PopulateContextTestData(context);
    }

    private static void PopulateContextTestData(ScenarioContext context)
    {
        context.Set(TableExtensions.CalculateAcademicYear("0"), ContextKeys.CurrentCollectionYear);
        context.Set(TableExtensions.Period[DateTime.Now.ToString("MMMM")], ContextKeys.CurrentCollectionPeriod);
    }
}