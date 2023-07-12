using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class AdaptorMessageHandler
    {
        private readonly ScenarioContext _context;
        private List<CalculatedRequiredLevyAmount> calculatedRequiredLevyAmountList;

        public AdaptorMessageHandler(ScenarioContext context)
        {
            _context = context;
        }

        public async Task ReceiveCalculatedRequiredLevyAmountEvent(string ULN)
        {
            await WaitHelper.WaitForIt(() =>
            {
                calculatedRequiredLevyAmountList =
                    CalculatedRequiredLevyAmountEventHandler.ReceivedEvents.Where(x => x.message.Learner.Uln.ToString() == ULN).Select(x => x.message).ToList();

                if (calculatedRequiredLevyAmountList != null)
                {
                    _context.Set(calculatedRequiredLevyAmountList);
                    return true;
                }
                return false;
            }, "Failed to find published 'Calculated Required Levy Amount' event in Payments");
        }
    }
}
