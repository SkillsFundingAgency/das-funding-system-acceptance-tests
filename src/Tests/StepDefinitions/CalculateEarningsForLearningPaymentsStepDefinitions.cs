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
    public void AnApprenticeshipIsCreatedWith(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice)
    {
        _messageHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice);
    }

    [When(@"the apprenticeship commitment is approved")]
    public async Task TheApprenticeshipCommitmentIsApproved()
    {
        await _messageHelper.PublishApprenticeshipApprovedMessage();
        await _messageHelper.ReadEarningsGeneratedMessage();
    }

    [Then(@"80% of the agreed price is calculated as total on-program payment which is divivded equally into number of planned months (.*)")]
    public void VerifyInstallmentAmountIsCalculatedEquallyIntoAllEarningMonths(decimal installmentAmount)
    {
        _context.Get<FundingPeriod>().DeliveryPeriods.ForEach(dp => dp.LearningAmount.Should().Be(installmentAmount));
    }

    [Then(@"the planned number of months must be the number of months from the start date to the planned end date (.*)")]
    public void VerifyThePlannedDurationMonthsWithinTheEarningsGenerated(short numberOfInstallments)
    {
        _context.Get<FundingPeriod>().DeliveryPeriods.Should().HaveCount(numberOfInstallments);
    }

    [Then(@"Earnings generated for each month starting from the first delivery period (.*)-(.*) and first calendar period (.*)/(.*)")]
    public void VerifyTheEarningsAreRecordedForEachMonthForTheWholeDuration(short firstDeliveryPeriodMonth, short firstDeliveryPeriodYear, short firstCalendarPeriodMonth, short firstCalendarPeriodYear)
    {
        var deliveryPeriods = _context.Get<FundingPeriod>().DeliveryPeriods;

        int numberOfInstallments = deliveryPeriods.Count;

        deliveryPeriods.ShouldHaveCorrectFundingPeriods(numberOfInstallments, firstDeliveryPeriodMonth, firstDeliveryPeriodYear);
        deliveryPeriods.ShouldHaveCorrectFundingCalendarMonths(numberOfInstallments, firstCalendarPeriodMonth, firstCalendarPeriodYear);
    }
}