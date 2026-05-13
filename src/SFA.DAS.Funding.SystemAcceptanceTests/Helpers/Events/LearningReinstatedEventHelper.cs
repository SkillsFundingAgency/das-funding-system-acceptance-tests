using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages.Events;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events
{
    internal static class LearningReinstatedEventHelper
    {
        internal static async Task ReceiveLearningReinstatedEvent (this ScenarioContext context, Guid learningKey)
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                LearningReinstatedEvent? learningReinstatedEvent =
                    LearningReinstatedEventHandler.GetMessage(x => x.LearningKey == learningKey);

                if (learningReinstatedEvent != null)
                {
                    testData.LearningReinstatedEvent = learningReinstatedEvent;
                    return true;
                }
                return false;
            }, "Failed to find published Learning Reinstated Event");
        }
    }
}
