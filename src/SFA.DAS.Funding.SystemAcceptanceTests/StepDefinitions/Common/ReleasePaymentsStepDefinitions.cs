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

    public ReleasePaymentsStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _paymentsFunctionsClient = new PaymentsFunctionsClient();
    }

    [Given(@"payments are released for (.*)")]
    [When(@"payments are released for (.*)")]
    public async Task ReleasePayments(TokenisableDateTime searchDate)
    {
        var releasePaymentsCommand = new ReleasePaymentsCommand();

        var period = TableExtensions.Period[searchDate.Value.ToString("MMMM")];
        var year = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", searchDate.Value));

        await _paymentsFunctionsClient.InvokeReleasePaymentsHttpTrigger(period, year);
    }
}
