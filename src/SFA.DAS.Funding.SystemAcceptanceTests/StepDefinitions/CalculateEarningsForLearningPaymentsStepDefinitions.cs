using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

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
    public void AnApprenticeshipIsCreatedWith(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice)
    {
        _messageHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice);
    }

    [Given(@"an apprenticeship has a start date of (.*), a planned end date of (.*), an agreed price of £(.*) and a funding band max as £(.*)")]
    public void AnApprenticeshipIsCreatedWith(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice, decimal fundingBandMax)
    {
        _messageHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice, fundingBandMax);
    }

    [When(@"the apprenticeship commitment is approved")]
    public async Task TheApprenticeshipCommitmentIsApproved()
    {
        await _messageHelper.PublishApprenticeshipApprovedMessage();
        await _messageHelper.ReadEarningsGeneratedMessage();
    }

    [Then(@"80% of the lowest value between agreed price and funding band price is divided equally into number of planned months (.*)")]
    [Then(@"80% of the agreed price is calculated as total on-program payment which is divided equally into number of planned months (.*)")]
    public void VerifyInstalmentAmountIsCalculatedEquallyIntoAllEarningMonths(decimal instalmentAmount)
    {
        _context.Get<FundingPeriod>().DeliveryPeriods.ForEach(dp => dp.LearningAmount.Should().Be(instalmentAmount));
    }

    [Then(@"the planned number of months must be the number of months from the start date to the planned end date (.*)")]
    public void VerifyThePlannedDurationMonthsWithinTheEarningsGenerated(short numberOfInstalments)
    {
        _context.Get<FundingPeriod>().DeliveryPeriods.Should().HaveCount(numberOfInstalments);
    }

    [Then(@"the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month/year")]
    public void ThenTheDeliveryPeriodForEachInstalmentMustBeTheDeliveryPeriodFromTheCollectionCalendarWithAMatchingCalendarMonthYear(Table table)
    {
        var deliveryPeriods = _context.Get<FundingPeriod>().DeliveryPeriods;

        deliveryPeriods.ShouldHaveCorrectFundingPeriods(table.ToExpectedPeriods());
    }
}