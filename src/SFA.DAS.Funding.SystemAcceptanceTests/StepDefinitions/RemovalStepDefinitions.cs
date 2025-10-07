using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class RemovalStepDefinitions(ScenarioContext context, LearnerDataOuterApiHelper learnerDataOuterApiHelper, EarningsSqlClient earningsSqlClient)
    {

        [When("sld inform us that the learner is to removed")]
        public async Task WhenSldInformUThatTheLearnerIsToRemoved()
        {
            var testData = context.Get<TestData>();
            await learnerDataOuterApiHelper.RemoveLearner(testData.EarningsGeneratedEvent.ApprenticeshipKey);
        }

        [Then("a learning removed event is published to approvals with reason (.*) and last day of learning as (.*)")]
        public async Task LearningRemovedEventIsPublishedToApprovals(string reason, TokenisableDateTime lastDayOfLearning)
        {
            var testData = context.Get<TestData>();

            await context.ReceiveLearningWithdrawnEvent(testData.LearningCreatedEvent.LearningKey);

            Assert.AreEqual(testData.LearningWithdrawnEvent.Reason, reason, "Unexpected withdrawal reason found in the event!");
            Assert.AreEqual(testData.LearningWithdrawnEvent.LastDayOfLearning.Date, lastDayOfLearning.Value.Date, "Unexpected last day of learning found in the event!");
        }
    }
}
