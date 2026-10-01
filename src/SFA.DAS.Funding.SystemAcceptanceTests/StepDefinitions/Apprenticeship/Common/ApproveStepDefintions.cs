using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Apprenticeship.Common;

[Binding]
public class ApproveStepDefintions(ScenarioContext context)
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
}
