using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
internal class ApprovalsStubStepDefinitions(ApprovalsStubClient approvalsStubClient)
{
    [Given("Approvals will auto-approve a start date change from (.*) to (.*) for a new learning key")]
    public async Task GivenApprovalsWillAutoApproveAStartDateChangeForANewLearningKey(TokenisableDateTime oldStartDate, TokenisableDateTime newStartDate)
    {
        await approvalsStubClient.SetUpAutoApproval(Guid.NewGuid(), oldStartDate.Value, newStartDate.Value);
    }
}
