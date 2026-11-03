using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Apprenticeship.Common;

[Binding]
public class ApproveStepDefintions(ScenarioContext context, ApprovalsStubClient approvalsStubClient)
{

    [Given(@"the apprenticeship learning is approved")]
    [When(@"the apprenticeship learning is approved")]
    [Given(@"the apprenticeship commitment is approved via the legacy route")]
    [When(@"the apprenticeship commitment is approved via the legacy route")]
    public async Task TheApprenticeshipCommitmentIsApproved()
    {
        var testData = context.Get<TestData>();
        await context.PublishApprenticeshipApprovedMessage(testData.CommitmentsApprenticeshipCreatedEvent);
    }

    [Given("Approvals will auto-approve a start date change from (.*) to (.*) for the learning")]
    [When("Approvals will auto-approve a start date change from (.*) to (.*) for the learning")]
    public async Task GivenApprovalsWillAutoApproveAStartDateChangeForTheLearning(TokenisableDateTime oldStartDate, TokenisableDateTime newStartDate)
    {
        var testData = context.Get<TestData>();
        await approvalsStubClient.SetUpAutoApproval(testData.LearningKey, oldStartDate.Value, newStartDate.Value);
    }
}
