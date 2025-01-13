﻿using SFA.DAS.Payments.FundingSource.Messages.Commands;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class AdaptorMessageHandler
    {
        private readonly ScenarioContext _context;

        public AdaptorMessageHandler(ScenarioContext context)
        {
            _context = context;
        }

        public async Task ReceiveCalculateOnProgrammePaymentEvent(string ULN, int expectedCount)
        {
            Task.Delay(10000).Wait();
            await WaitHelper.WaitForItAsync(async () =>
            {
                var calculatedOnProgrammePaymentList = await CalculateOnProgrammePaymentEventHandler.ReceivedEvents<CalculateOnProgrammePayment>(x => x.Learner.Uln.ToString() == ULN);
                //CalculateOnProgrammePaymentEventHandler.ReceivedEvents.Where(x => x.message.Learner.Uln.ToString() == ULN).Select(x => x.message).ToList();

                if (calculatedOnProgrammePaymentList.Count != expectedCount) return false;

                _context.Set(calculatedOnProgrammePaymentList);

                return calculatedOnProgrammePaymentList.All(x => x.Learner.Uln.ToString() == ULN);

            }, $"Failed to find published 'Calculated Required Levy Amount' event in Payments. Expected Count: {expectedCount}");
        }
    }
}
