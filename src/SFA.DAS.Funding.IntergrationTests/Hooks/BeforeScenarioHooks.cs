using SFA.DAS.Funding.IntegrationTests.Infrastructure;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.AzureDurableFunctions;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.IntegrationTests.Hooks
{
    [Binding]
    public class BeforeScenarioHooks
    {
        [BeforeScenario(Order = 1)]
        public void SetUpHelpers(ScenarioContext context)
        {
            var config = new FundingConfig();
            context.Set(config);
            context.Set(new FundingOrchestrationHelper(config));
            context.Set(new Helper(context));
        }
    }
}