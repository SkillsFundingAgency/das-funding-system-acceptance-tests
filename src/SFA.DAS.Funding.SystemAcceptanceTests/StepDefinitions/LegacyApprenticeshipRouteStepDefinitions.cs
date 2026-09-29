using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class LegacyApprenticeshipRouteStepDefinitions(ScenarioContext context, LearningSqlClient learningSqlClient, EarningsSqlClient earningsSqlClient)
{
    [Then(@"the learning is created")]
    public void ThenTheLearningIsCreated()
    {
        var testData = context.Get<TestData>();
        var apprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        var learning = learningSqlClient.GetApprenticeshipByUln(apprenticeshipCreatedEvent.Uln);

        Assert.IsNotNull(learning, "Expected a Learning record to be present in the database");
        Assert.IsTrue(learning.TrainingCode.Trim() == apprenticeshipCreatedEvent.TrainingCode &&
                learning.Episodes.Any(e => e.Ukprn == apprenticeshipCreatedEvent.ProviderId),
            "Expected a matching episode to be present on the Learning record");

        testData.LearningKey = learning.Key;
    }

    [Then(@"no earnings are generated for the apprenticeship")]
    public void ThenNoEarningsAreGeneratedForTheApprenticeship()
    {
        var testData = context.Get<TestData>();

        var earnings = earningsSqlClient.GetApprenticeshipEarningsEntityModel(testData.LearningKey);

        Assert.IsNull(earnings, "Expected no Earnings record to exist for this Learning, but one was found");
    }
}
