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

    [Given(@"pause (.*) seconds")]
    public async Task Pause(int seconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
    }

    [Given("Payments are released")]
    [When(@"the scheduler triggers Unfunded Payment processing")]
    [Then(@"the scheduler triggers Unfunded Payment processing")]
    public async Task ReleasePayments()
    {
        var testData = _context.Get<TestData>();

        await _paymentsFunctionsClient.InvokeReleasePaymentsHttpTrigger(_context, testData.CurrentCollectionPeriod,
            Convert.ToInt16(testData.CurrentCollectionYear));
    }

    [When(@"the Release Payments command is published again")]
    public async Task ReleasePaymentsAgain()
    {
        var testData = _context.Get<TestData>();
        FinalisedOnProgrammeLearningPaymentEventHandler.Clear(x => x.ApprenticeshipKey == testData.LearningKey);

        await _paymentsFunctionsClient.InvokeReleasePaymentsHttpTrigger(_context, testData.CurrentCollectionPeriod,
            Convert.ToInt16(testData.CurrentCollectionYear));
    }
}
