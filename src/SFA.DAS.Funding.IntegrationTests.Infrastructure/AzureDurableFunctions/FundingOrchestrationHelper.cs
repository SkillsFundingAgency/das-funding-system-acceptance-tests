using SFA.DAS.Funding.IntegrationTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.IntegrationTests.Infrastructure.AzureDurableFunctions
{
    public class FundingOrchestrationHelper : FundingFunctionAppHelper
    {
        public FundingOrchestrationHelper(FundingConfig config) : base(config)
        {
        }

        public async Task StartResponseFundingsOrchestrator()
        {
            await StartOrchestrator("api/orchestrators/ResponseOrchestrator");
        }
        
        public async Task StartFundingsOrchestrator()
        {
            await StartOrchestrator("api/orchestrators/FundingsOrchestrator", true);
        }

        public async Task WaitUntilComplete(TimeSpan? timeout = null)
        {
            await WaitUntilStatus(timeout ?? TimeSpan.FromMinutes(2), false,"Completed");
        }

        public async Task WaitUntilStopped(TimeSpan? timeout = null)
        {
            await WaitUntilStatus(timeout ?? TimeSpan.FromMinutes(5), true, "Completed", "Failed");
        }
    }
}
