using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class CalculateEarningsForLearningPaymentsStepDefinitions
{
    private readonly ScenarioContext _context;
    private ServiceBusMessageHelper _messageHelper;
    private EarningsGeneratedEvent _earnings;
    private FundingPeriod _fundingPeriod;

    public CalculateEarningsForLearningPaymentsStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _messageHelper = new ServiceBusMessageHelper(_context);
    }

    [Given(@"an apprenticeship has a start date of (.*), a planned end date of (.*), and an agreed price of £(.*)")]
    public void AnApprenticeshipIsCreatedWith(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice)
    {
        var apprenticeshipEvent = _messageHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice);
        _context.Set(apprenticeshipEvent);
    }

    [When(@"the agreed price is below the funding band maximum (.*) for the selected course")]
    [When(@"the agreed price is above the funding band maximum (.*) for the selected course")]
    public void SetFundingBandMaxValue(Decimal fundingBandMax)
    {
        var apprenticeshipEvent = _context.Get<ApprenticeshipCreatedEvent>();
        _messageHelper.UpdateApprenticeshipCreatedMessageWithFundingBandMaximumValue(apprenticeshipEvent, fundingBandMax);
    }

    [When(@"the apprenticeship commitment is approved")]
    public async Task TheApprenticeshipCommitmentIsApproved()
    {
        var apprenticeshipEvent = _context.Get<ApprenticeshipCreatedEvent>();
        await _messageHelper.PublishApprenticeshipApprovedMessage(apprenticeshipEvent);
        _earnings = _messageHelper.ReadEarningsGeneratedMessage(apprenticeshipEvent);
        _fundingPeriod = _earnings.FundingPeriods.First();
        
        _context.Set(_earnings);
        _context.Set(_fundingPeriod);
    }

    [Then(@"80% of the agreed price is calculated as total on-program payment which is divided equally into number of planned months (.*)")]
    [Then(@"Agreed price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
    [Then(@"Funding band maximum price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
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