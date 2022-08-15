using Microsoft.VisualStudio.TestPlatform.TestHost;
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
    public void AnApprenticeshipIsCreatedWith(DateTime start_date, DateTime planned_end_date, decimal agreed_price)
    {
        _messageHelper.CreateAnApprenticeshipMessage(start_date, planned_end_date, agreed_price);
    }

    [When(@"the apprenticeship commitment is approved")]
    public async Task TheApprenticeshipCommitmentIsApproved()
    {
        await _messageHelper.PublishAnApprenticeshipApprovedMessage();
        await _messageHelper.ReadEarningsGeneratedMessage();
    }

    [Then(@"the total on-program payment amount must be calculated as 80% of the agreed price £(.*)")]
    public void TheTotalOnProgramPaymentAmountMustBeCalculatedAs80PercentOfTheAgreedPrice(decimal adjustedPrice)
    {
        _context.Get<EarningsGeneratedEvent>().FundingPeriods.Should().HaveAdjustedAgreedPriceOf(adjustedPrice);
    }

    [Then(@"the planned number of months must be the number of months from the start date to the planned end date (.*)")]
    public void VerifyThePlannedDurationMonthsWithinTheEarningsGenerated(short numberOfInstallments)
    {
        _context.Get<FundingPeriod>().DeliveryPeriods.Should().HaveCount(numberOfInstallments);
    }

    [Then(@"the instalment amount must be calculated by dividing the total on-program amount equally into the number of planned months (.*)")]
    public void VerifyInstallmentAmountIsCalculatedEquallyIntoAllEarningMonths(decimal installmentAmount)
    {
        _context.Get<FundingPeriod>().DeliveryPeriods.ForEach(dp => dp.LearningAmount.Should().Be(installmentAmount));
    }

    [Then(@"an earning must be recorded for each month from the start date to the planned end date")]
    public void VerifyTheEarningsAreRecordedForEachMonthForTheWholeDuration()
    {
        _context.Get<FundingPeriod>().DeliveryPeriods.ShouldHaveCorrectFundingPeriods(1,1,1);
    }

    //The following 2 methods are for the next set of tickets
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

    private decimal CalculateOnProgramPaymentBasedOnAgreedPriceAndFundingBand(decimal agreed_price)
    {
        return agreed_price * 0.80m;
    }
}