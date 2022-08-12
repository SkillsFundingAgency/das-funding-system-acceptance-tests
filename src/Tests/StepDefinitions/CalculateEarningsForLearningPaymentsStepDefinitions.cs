using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using System.Globalization;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class CalculateEarningsForLearningPaymentsStepDefinitions
{
    private readonly ScenarioContext _context;
    private ServiceBusMessageHelper _messageHelper;

    public CalculateEarningsForLearningPaymentsStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _messageHelper = new ServiceBusMessageHelper(_context);
    }

    [Given(@"an apprenticeship has a start date of (.*), a planned end date of (.*), and an agreed price of £(.*)")]
    public void GivenAnApprenticeshipIsCreatedWith(String start_date, String planned_end_date, String agreed_price)
    {
        //_messageHelper.CreateAnApprenticeshipMessage(start_date, planned_end_date, agreed_price);
    }

    [When(@"the apprenticeship commitment is approved")]
    public async Task ApprenticeshipCommitmentIsApproved()
    {
        await _messageHelper.PublishAnApprenticeshipApprovedMessage();
        await _messageHelper.ReadEarningsGeneratedMessage();
    }

    public void verification1()
    {
        _context.Get<EarningsGeneratedEvent>().Should().Be("something");
    }

    public void verification2()
    {

    }

    private DateTime ConvertToDateTimeFormat(String requiredDate)
    {
        int month = DateTime.ParseExact(requiredDate.Split('-')[0], "MMM", CultureInfo.CurrentCulture).Month;
        String yearPart = requiredDate.Split('-')[1];
        var year = yearPart switch
        {
            "CurrentYear" => DateTime.Now.Year,
            "NextYear" => DateTime.Now.Year + 1,
            _ => throw new Exception("Unsupported format"),
        };
        return new DateTime(01, month, year);
    }
}