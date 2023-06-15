using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class ConvertUnfundedPaymentsDataToPaymentsV2FormatStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly AdaptorMessageHandler _adaptorMessageHelper;

        public ConvertUnfundedPaymentsDataToPaymentsV2FormatStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _adaptorMessageHelper = new AdaptorMessageHandler(_context);
        }

        [Then(@"the Calculated Required Levy Amount event is published with default values")]
        public async Task ThenTheCalculatedRequiredLevyAmountEventIsPublishedWithDefaultValues()
        {
            await _adaptorMessageHelper.ReceiveCalculatedRequiredLevyAmountEvent(_context.Get<ApprenticeshipCreatedEvent>().Uln);
        }

    }
}
