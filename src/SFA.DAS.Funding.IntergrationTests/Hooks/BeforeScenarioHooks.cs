using SFA.DAS.Funding.IntegrationTests.Infrastructure;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.AzureDurableFunctions;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.IntegrationTests.Hooks
{
    [Binding]
    public class BeforeScenarioHooks
    {
        private readonly ScenarioContext _context;

        public BeforeScenarioHooks(ScenarioContext context)
        {
            _context = context;
        }

        [BeforeScenario(Order = 1)]
        public void SetUpHelpers()
        {
            var config = new FundingConfig();
            _context.Set(config);
            _context.Set(new FundingOrchestrationHelper(config));
            _context.Set(new Helper(_context));
        }
           
    }
}