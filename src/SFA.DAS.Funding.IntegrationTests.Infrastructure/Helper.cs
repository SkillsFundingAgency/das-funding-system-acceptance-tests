using SFA.DAS.Funding.IntegrationTests.Infrastructure.AzureDurableFunctions;
using TechTalk.SpecFlow;

namespace SFA.DAS.Funding.IntegrationTests.Infrastructure
{
    public class Helper
    {
        public FundingOrchestrationHelper FundingOrchestrationHelper => _context.Get<FundingOrchestrationHelper>();
        private readonly ScenarioContext _context;

        public Helper(ScenarioContext context)
        {
            _context = context;
            context.Set(new FundingOrchestratorHelper(context));
        }
    }
}
