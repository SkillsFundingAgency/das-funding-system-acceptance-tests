using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    CalculatedRequiredLevyAmountEventHandler.ReceivedEvents.FirstOrDefault(x => x.Learner.Uln == long.Parse(ULN));

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
