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

    [Given(@"the Unfunded Payments for the remainder of the apprenticeship are determined")]
    [When(@"the Unfunded Payments for the remainder of the apprenticeship are determined")]
    [Given(@"Payments Generated Events are published")]
    [When(@"Payments Generated Events are published")]
    [Then(@"Payments Generated Events are published")]
    public async Task SetPaymentsGeneratedInScenarioContext()
    {
        var testData = _context.Get<TestData>();

        await WaitHelper.WaitForIt(() =>
        {
            var paymentsEvent =
                PaymentsGeneratedEventHandler.GetMessage(x => x.ApprenticeshipKey == testData.LearningKey);

            if (paymentsEvent != null)
            {
                var testData = _context.Get<TestData>();
                testData.PaymentsGeneratedEvent = paymentsEvent;
                return true;
            }
            return false;
        }, "Failed to find published event in Payments");
    }
}