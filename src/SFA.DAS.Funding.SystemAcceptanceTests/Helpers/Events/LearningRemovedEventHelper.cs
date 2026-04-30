using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages.Events;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events
{
    internal static class LearningRemovedEventHelper
    {
        internal static async Task ReceiveLearningRemovedEvent (this ScenarioContext context, Guid learningKey)
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                LearningRemovedEvent? learningRemovedEvent =
                    LearningRemovedEventHandler.GetMessage(x => x.LearningKey == learningKey);

                if (learningRemovedEvent != null)
                {
                    testData.LearningRemovedEvent = learningRemovedEvent;
                    return true;
                }
                return false;
            }, "Failed to find published Learning Removed Event");
        }
    }
}
