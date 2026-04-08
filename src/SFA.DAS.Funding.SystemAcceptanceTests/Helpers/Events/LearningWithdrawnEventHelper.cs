using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages.Events;
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
                ApprenticeshipWithdrawnEvent? apprenticeshipWithdrawnEvent =
                    ApprenticeshipWithdrawnEventHandler.GetMessage(x => x.LearningKey == learningKey);

                if (apprenticeshipWithdrawnEvent != null)
                {
                    testData.ApprenticeshipWithdrawnEvent = apprenticeshipWithdrawnEvent;
                    return true;
                }
                return false;
            }, "Failed to find published Apprenticeship Withdrawn Event");
        }
    }
}
