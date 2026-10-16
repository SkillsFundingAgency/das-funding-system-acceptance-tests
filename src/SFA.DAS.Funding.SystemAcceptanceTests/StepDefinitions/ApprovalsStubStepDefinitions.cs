using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
internal class ApprovalsStubStepDefinitions(ScenarioContext context, ApprovalsStubClient approvalsStubClient)
{
    [Given("Approvals will auto-approve a start date change from (.*) to (.*) for the learning")]
    public async Task GivenApprovalsWillAutoApproveAStartDateChangeForTheLearning(TokenisableDateTime oldStartDate, TokenisableDateTime newStartDate)
    {
        var testData = context.Get<TestData>();
        await approvalsStubClient.SetUpAutoApproval(testData.LearningKey, oldStartDate.Value, newStartDate.Value);
    }
}
