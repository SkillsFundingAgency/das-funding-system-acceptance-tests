using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events
{
    internal static class LearningWithdrawnEventHelper
    {
        internal static async Task ReceiveLearningWithdrawnEvent (this ScenarioContext context, Guid learningKey)
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                LearningWithdrawnEvent? learningWithdrawnEvent =
                    LearningWithdrawnEventHandler.GetMessage(x => x.LearningKey == learningKey);

                if (learningWithdrawnEvent != null)
                {
                    testData.LearningWithdrawnEvent = learningWithdrawnEvent;
                    return true;
                }
                return false;
            }, "Failed to find published Learning Withdrawn Event");
        }
    }
}
