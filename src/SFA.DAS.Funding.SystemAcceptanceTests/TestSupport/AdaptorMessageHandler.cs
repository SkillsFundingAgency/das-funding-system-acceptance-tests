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

        public async Task ReceiveCalculatedRequiredLevyAmountEvent(string ULN)
        {
            await WaitHelper.WaitForIt(() =>
            {
                CalculatedRequiredLevyAmount? calculatedRequiredLevyAmount =
                    CalculatedRequiredLevyAmountEventHandler.ReceivedEvents.FirstOrDefault().message;//x => x.EventTime.DateTime.ToUniversalTime() >= DateTime.UtcNow.AddMinutes(-5));

                if (calculatedRequiredLevyAmount != null)
                {
                    _context.Set(calculatedRequiredLevyAmount);
                    return true;
                }
                return false;
            }, "Failed to find published 'Calculated Required Levy Amount' event in Payments");
        }
    }
}
