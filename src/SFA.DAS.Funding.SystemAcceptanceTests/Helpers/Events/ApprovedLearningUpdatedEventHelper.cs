using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages.Events;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class ApprovedLearningUpdatedEventHelper
{
    internal static async Task ReceiveApprovedLearningUpdatedEvent(this ScenarioContext context, Guid learningKey)
    {
        var testData = context.Get<TestData>();

        await WaitHelper.WaitForIt(() =>
        {
            ApprovedLearningUpdatedEvent? approvedLearningUpdatedEvent =
                ApprovedLearningUpdatedEventHandler.GetMessage(x => x.LearningKey == learningKey);

            if (approvedLearningUpdatedEvent != null)
            {
                testData.ApprovedLearningUpdatedEvent = approvedLearningUpdatedEvent;
                return true;
            }

            return false;
        }, "Failed to find published Approved Learning Updated Event");
    }
}
