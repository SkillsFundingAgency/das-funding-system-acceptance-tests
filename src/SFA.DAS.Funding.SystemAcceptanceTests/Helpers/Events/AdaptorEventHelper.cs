using SFA.DAS.Payments.FundingSource.Messages.Commands;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class AdaptorEventHelper
{
    internal static async Task ReceiveCalculateOnProgrammePaymentEvent(this ScenarioContext context, string ULN, int expectedCount)
    {
        var testData = context.Get<TestData>();

        await WaitHelper.WaitForItAsync(async () =>
        {
            var calculatedOnProgrammePaymentList = await CalculateOnProgrammePaymentEventHandler.ReceivedEvents<CalculateOnProgrammePayment>(x => x.Learner.Uln.ToString() == ULN);

            if (calculatedOnProgrammePaymentList.Count != expectedCount) return false;

            testData.CalculatedOnProgrammePaymentList = calculatedOnProgrammePaymentList;

            return calculatedOnProgrammePaymentList.All(x => x.Learner.Uln.ToString() == ULN);

        }, $"Failed to find published 'Calculated Required Levy Amount' event in Payments. Expected Count: {expectedCount}");
    }
}