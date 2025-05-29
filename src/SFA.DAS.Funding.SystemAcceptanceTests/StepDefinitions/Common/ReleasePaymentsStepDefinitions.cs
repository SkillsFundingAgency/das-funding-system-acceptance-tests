using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Common;

[Binding]
public class ReleasePaymentsStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly PaymentsFunctionsClient _paymentsFunctionsClient;

    public ReleasePaymentsStepDefinitions(ScenarioContext context, PaymentsFunctionsClient paymentsFunctionsClient)
    {
        _context = context;
        _paymentsFunctionsClient = paymentsFunctionsClient;
    }

    [When(@"the scheduler triggers Unfunded Payment processing")]
    [Then(@"the scheduler triggers Unfunded Payment processing")]
    public async Task ReleasePayments()
    {
        var testData = _context.Get<TestData>();

        await _paymentsFunctionsClient.InvokeReleasePaymentsHttpTrigger(testData.CurrentCollectionPeriod,
            Convert.ToInt16(testData.CurrentCollectionYear));

        await Task.Delay(10000);
    }
}
