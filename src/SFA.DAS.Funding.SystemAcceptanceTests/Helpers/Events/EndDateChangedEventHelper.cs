using Reqnroll;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events
{
    internal static class EndDateChangedEventHelper
    {
        internal static async Task ReceiveEndDateChangedEvent (this ScenarioContext context, Guid learningKey)
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                EndDateChangedEvent? endDateChangedEvent =
                    EndDateChangedEventHandler.GetMessage(x => x.LearningKey == learningKey);

                if (endDateChangedEvent != null)
                {
                    testData.EndDateChangedEvent = endDateChangedEvent;
                    return true;
                }
                return false;
            }, "Failed to find published End Date Changed Event");
        }
    }
}
