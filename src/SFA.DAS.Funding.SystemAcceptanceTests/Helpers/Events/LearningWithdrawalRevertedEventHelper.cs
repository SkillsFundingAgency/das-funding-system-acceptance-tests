using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages.Events;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events
{
    internal static class LearningWithdrawalRevertedEventHelper
    {
        internal static async Task ReceiveWithdrawalRevertedEvent(this ScenarioContext context, Guid learningKey)
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                SFA.DAS.Learning.Types.LearningWithdrawalRevertedEvent? learningWithdrawalRevertedEvent =
                    LearningWithdrawalRevertedEventHandler.GetMessage(x => x.LearningKey == learningKey);

                if (learningWithdrawalRevertedEvent != null)
                {
                    testData.WithdrawalRevertedEvent = learningWithdrawalRevertedEvent;
                    return true;
                }
                return false;
            }, "Failed to find published Withdrawal Reverted Event");
        }
    }
}
