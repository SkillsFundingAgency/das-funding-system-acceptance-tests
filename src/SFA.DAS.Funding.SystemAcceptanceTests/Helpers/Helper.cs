using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.AzureDurableFunctions;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

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