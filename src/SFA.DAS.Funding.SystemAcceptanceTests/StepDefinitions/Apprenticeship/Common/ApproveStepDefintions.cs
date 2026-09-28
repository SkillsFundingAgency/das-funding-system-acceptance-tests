using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Apprenticeship.Common;

[Binding]
public class ApproveStepDefintions(ScenarioContext context, EarningsSqlClient earningsSqlClient, LearningSqlClient learningSqlClient, LearnerDataOuterApiHelper learnerDataOuterApiHelper)
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
