using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class AdaptorMessageHandler
    {
        private readonly ScenarioContext _context;

        public AdaptorMessageHandler(ScenarioContext context)
        {
            _context = context;
        }

        public async Task ReceiveCalculatedRequiredLevyAmountEvent(string ULN, int expectedCount)
        {
            await WaitHelper.WaitForIt(() =>
            {
                var calculatedRequiredLevyAmountList =
                    CalculatedRequiredLevyAmountEventHandler.ReceivedEvents.Where(x => x.message.Learner.Uln.ToString() == ULN).Select(x => x.message).ToList();

                if (calculatedRequiredLevyAmountList.Count != expectedCount) return false;

                _context.Set(calculatedRequiredLevyAmountList);

                return calculatedRequiredLevyAmountList.All(x=> x.Learner.Uln.ToString() == ULN);
   
            }, $"Failed to find published 'Calculated Required Levy Amount' event in Payments. Expected Count: {expectedCount}");
        }
    }
}
