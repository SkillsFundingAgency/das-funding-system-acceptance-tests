using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Apprenticeship.Common;

[Binding]
public class UpdateStepDefinitions(ScenarioContext context, LearnerDataOuterApiHelper learnerDataOuterApiHelper, LearnerDataSqlClient learnerDataSqlClient, LearningSqlClient learningSqlClient)
{

    [Given(@"SLD submit updated learners details")]
    [When(@"SLD submit updated learners details")]
    public async Task WhenSLDSubmitUpdatedLearnersDetails()
    {
        var testData = context.Get<TestData>();

        if (testData.LearnerDataBuilder == null)
        {
            throw new InvalidOperationException(
                "No learner data builder has been stored; cannot build or submit learner data");
        }

        var learnerData = testData.LearnerDataBuilder.Build();

        testData.UpdateLearnerData = await learnerDataOuterApiHelper.UpdateLearning(testData.LearnerKey, learnerData);
    }

}
