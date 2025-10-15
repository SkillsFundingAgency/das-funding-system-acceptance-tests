using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events
{
    internal static class WithdrawalRevertedEventHelper
    {
        internal static async Task ReceiveWithdrawalRevertedEvent(this ScenarioContext context, Guid learningKey)
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                SFA.DAS.Learning.Types.WithdrawalRevertedEvent? withdrawalRevertedEvent =
                    WithdrawalRevertedEventHandler.GetMessage(x => x.LearningKey == learningKey);

                if (withdrawalRevertedEvent != null)
                {
                    testData.WithdrawalRevertedEvent = withdrawalRevertedEvent;
                    return true;
                }
                return false;
            }, "Failed to find published Withdrawal Reverted Event");
        }
    }
}
