using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class FinalisedOnProgrammeLearningPaymentEventHelper
{
    internal static async Task UnpaidUnfundedPaymentsForTheCurrentCollectionMonthAndRollupPaymentsAreSentToBePaid(this ScenarioContext context, int numberOfRollupPayments)
    {
        var testData = context.Get<TestData>();

        await WaitHelper.WaitForIt(() =>
        {
            var finalisedPaymentsList =
                FinalisedOnProgrammeLearningPaymentEventHandler.ReceivedEvents.Where(x => x.message.ApprenticeshipKey == testData.ApprenticeshipKey).Select(x => x.message).ToList();

            if (finalisedPaymentsList.Count != numberOfRollupPayments + 1) return false;

            context.Set(finalisedPaymentsList);
            
            return finalisedPaymentsList.All(x => x.CollectionPeriod == testData.CurrentCollectionPeriod);
        }, "Failed to find published Finalised On Programme Learning Payment event");
    }
}
