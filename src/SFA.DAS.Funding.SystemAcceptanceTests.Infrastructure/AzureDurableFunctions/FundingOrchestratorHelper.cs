using TechTalk.SpecFlow;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.AzureDurableFunctions
{
    public class FundingOrchestratorHelper
    {
        private readonly FundingOrchestrationHelper _checkOrchestrationHelper;

        public FundingOrchestratorHelper(ScenarioContext context)
        {
            _checkOrchestrationHelper = context.Get<FundingOrchestrationHelper>();
        }

        public async Task Run(bool continueOnFailure = false)
        {
            await _checkOrchestrationHelper.StartResponseFundingsOrchestrator();
            if (continueOnFailure) await _checkOrchestrationHelper.WaitUntilStopped();
            else await _checkOrchestrationHelper.WaitUntilComplete();
        }
    }
}
