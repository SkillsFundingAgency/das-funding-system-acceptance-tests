using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events
{
    internal static class PersonalDetailsChangedEventHelper
    {
        internal static async Task ReceivePersonalDetailsChangedEvent(this ScenarioContext context, Guid learningKey)
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                SFA.DAS.Learning.Types.PersonalDetailsChangedEvent? personalDetailsChangedEvent =
                    PersonalDetailsChangedEventHandler.GetMessage(x => x.LearningKey == learningKey);

                if (personalDetailsChangedEvent != null)
                {
                    testData.PersonalDetailsChangedEvent = personalDetailsChangedEvent;
                    return true;
                }
                return false;
            }, "Failed to find published Personal Details Changed Event");
        }
    }
}
