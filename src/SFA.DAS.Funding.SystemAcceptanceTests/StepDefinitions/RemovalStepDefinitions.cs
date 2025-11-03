using FluentAssertions.Execution;
using Reqnroll;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class RemovalStepDefinitions(ScenarioContext context, LearnerDataOuterApiHelper learnerDataOuterApiHelper, EarningsSqlClient earningsSqlClient)
    {
        [Given("sld inform us that the learner is to removed")]
        [When("sld inform us that the learner is to removed")]
        public async Task WhenSldInformUThatTheLearnerIsToRemoved()
        {
            var testData = context.Get<TestData>();
            await learnerDataOuterApiHelper.RemoveLearner(testData.EarningsGeneratedEvent.ApprenticeshipKey);

            testData.LastDayOfLearning = testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate;
        }

        [Given("a learning withdrawn event is published to approvals with reason (.*) and last day of learning as (.*)")]
        [Then("a learning withdrawn event is published to approvals with reason (.*) and last day of learning as (.*)")]
        public async Task LearningWithdrawnEventIsPublishedToApprovals(string reason, TokenisableDateTime lastDayOfLearning)
        {
            var testData = context.Get<TestData>();

            await context.ReceiveLearningWithdrawnEvent(testData.LearningCreatedEvent.LearningKey);

            Assert.AreEqual(reason, testData.LearningWithdrawnEvent.Reason, "Unexpected withdrawal reason found in the event!");
            Assert.AreEqual(lastDayOfLearning.Value.Date, testData.LearningWithdrawnEvent.LastDayOfLearning.Date, "Unexpected last day of learning found in the event!");
        }

        [When("a withdrawal reverted event is published to approvals")]
        public async Task WithdrawalRevertedEventIsPublishedToApprovals()
        {
            var testData = context.Get<TestData>();

            await context.ReceiveWithdrawalRevertedEvent(testData.LearningCreatedEvent.LearningKey);

            Assert.AreEqual(testData.LearningCreatedEvent.ApprovalsApprenticeshipId, testData.WithdrawalRevertedEvent.ApprovalsApprenticeshipId, "Unexpected approvals apprenticeship Id found in the event!");
        }

    }
}
