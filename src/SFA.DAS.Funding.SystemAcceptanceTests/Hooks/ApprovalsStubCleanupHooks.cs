using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

/// <summary>
/// Removes the Approvals mappings a scenario set up in das-api-stub. Each mapping is keyed by learning key, so without
/// this they would pile up in the stub's table, which every team using that environment shares.
/// This is only tidying up, so a failed removal is logged and does not fail the scenario.
/// </summary>
[Binding]
internal class ApprovalsStubCleanupHooks
{
    [AfterScenario]
    public async Task RemoveApprovalSetUps(ApprovalsStubClient approvalsStubClient)
    {
        foreach (var learningKey in approvalsStubClient.LearningKeysSetUp.ToList())
        {
            try
            {
                await approvalsStubClient.RemoveApprovalSetUp(learningKey);
            }
            catch (Exception ex)
            {
                LoggerHelper.WriteLog($"Could not remove the Approvals stub mapping for learning {learningKey}: {ex.Message}");
            }
        }
    }
}
