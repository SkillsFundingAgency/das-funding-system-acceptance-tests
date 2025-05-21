using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class FinalisedOnProgrammeLearningPaymentEventHelper
{
    internal static async Task UnpaidUnfundedPaymentsForTheCurrentCollectionMonthAndRollupPaymentsAreSentToBePaid(this ScenarioContext context, int numberOfRollupPayments)
    {
        var currentCollectionPeriod = context.Get<byte>(ContextKeys.CurrentCollectionPeriod);

        await WaitHelper.WaitForIt(() =>
        {
            var finalisedPaymentsList =
                FinalisedOnProgrammeLearningPaymentEventHandler.ReceivedEvents.Where(x => x.message.ApprenticeshipKey == context.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey).Select(x => x.message).ToList();

            if (finalisedPaymentsList.Count != numberOfRollupPayments + 1) return false;

            context.Set(finalisedPaymentsList);
            
            return finalisedPaymentsList.All(x => x.CollectionPeriod == currentCollectionPeriod);
        }, "Failed to find published Finalised On Programme Learning Payment event");
    }
}
