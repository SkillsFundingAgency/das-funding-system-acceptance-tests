using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Common;

[Binding]
public class PaymentsGeneratedStepDefinitions
{
    private readonly ScenarioContext _context;

    public PaymentsGeneratedStepDefinitions(ScenarioContext context, PaymentsFunctionsClient paymentsFunctionsClient)
    {
        _context = context;
    }
}