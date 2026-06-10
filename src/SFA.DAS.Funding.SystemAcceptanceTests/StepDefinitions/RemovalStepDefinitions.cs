using FluentAssertions.Execution;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class RemovalStepDefinitions(ScenarioContext context, LearnerDataOuterApiHelper learnerDataOuterApiHelper, EarningsSqlClient earningsSqlClient)
    {
        private static string UniversalWithdrawalReason = "WithdrawDuringLearning";

        [Given("sld inform us that the learner is to removed")]
        [When("sld inform us that the learner is to removed")]
        public async Task WhenSldInformUThatTheLearnerIsToRemoved()
        {
            var testData = context.Get<TestData>();
            var apprenticeshipKey = testData.EarningsGeneratedEvent.ApprenticeshipKey;
            ApprenticeshipEarningsRecalculatedEventHandler.Clear(x => x.ApprenticeshipKey == apprenticeshipKey);
            await learnerDataOuterApiHelper.RemoveLearner(apprenticeshipKey);
        }

        [Given("a learning withdrawn event is published to approvals with last day of learning as (.*)")]
        [Then("a learning withdrawn event is published to approvals with last day of learning as (.*)")]
        public async Task LearningWithdrawnEventIsPublishedToApprovals(TokenisableDateTime lastDayOfLearning)
        {
            var testData = context.Get<TestData>();

            await context.ReceiveLearningWithdrawnEvent(testData.LearningCreatedEvent.LearningKey);

            Assert.AreEqual(lastDayOfLearning.Value.Date, testData.LearningWithdrawnEvent.WithdrawalDate.Date, "Unexpected last day of learning found in the event!");
        }

        [Then("a withdrawal reason (.*) is sent to approvals in the learning withdrawn event")]
        public void ThenAWithdrawalReasonReasonIsSentToApprovalsInTheLearningWithdrawnEvent(short reasonCode)
        {
            var testData = context.Get<TestData>();

            testData.LearningWithdrawnEvent.WithdrawalReasonCode.Should().Be(reasonCode, "Unexpected withdrawal reason code found in the event!");
        }


        [Given("a learning removed event is published to approvals")]
        [Then("a learning removed event is published to approvals")]
        public async Task LearningRemovedEventIsPublishedToApprovals()
        {
            var testData = context.Get<TestData>();

            var learningKey = testData.LearningCreatedEvent?.LearningKey;

            var learningKeyToUse = learningKey.HasValue && learningKey.Value != Guid.Empty
                ? learningKey.Value
                : testData.ShortCourseLearningKey;

            await context.ReceiveLearningRemovedEvent(learningKeyToUse);

        }

        [Then("a learning reinstated event is published to approvals")]
        public async Task LearningReinstatedEventIsPublishedToApprovals()
        {
            var testData = context.Get<TestData>();

            var learningKey = testData.LearningCreatedEvent?.LearningKey;

            var learningKeyToUse = learningKey.HasValue && learningKey.Value != Guid.Empty
                ? learningKey.Value
                : testData.ShortCourseLearningKey;

            await context.ReceiveLearningReinstatedEvent(learningKeyToUse);
        }



        [Then("the apprentice is not returned in the GetLearners response")]
        public void ApprenticeIsNotReturnedInGetLearnersResponse()
        {
            var testData = context.Get<TestData>();
            var learners = testData.LearnersOnService;

            Assert.IsNotNull(learners, "LearnersOnService was not populated — call the Get Learners step first.");
            Assert.IsFalse(
                learners.Learners.Any(l => l.Uln == testData.Uln),
                $"Removed apprentice with ULN {testData.Uln} was unexpectedly returned in the GetLearners response.");
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
