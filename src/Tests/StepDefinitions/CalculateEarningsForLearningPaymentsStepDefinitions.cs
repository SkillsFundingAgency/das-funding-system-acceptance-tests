using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class CalculateEarningsForLearningPaymentsStepDefinitions
{
    private readonly ScenarioContext _context;
    private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
    private EarningsGeneratedEvent _earnings;

    public CalculateEarningsForLearningPaymentsStepDefinitions(ScenarioContext context)
    {
        _context = context;
    }

    [Given(@"An apprenticeship is created with (.*), (.*), (.*)")]
    public async Task GivenAnApprenticeshipIsCreatedWith(decimal agreedPrice, DateTime actualStartDate,
        DateTime plannedEndDate)
    {
        var fixture = new Fixture();
        _apprenticeshipCreatedEvent = fixture.Build<ApprenticeshipCreatedEvent>()
            .With(_ => _.AgreedPrice, agreedPrice)
            .With(_ => _.ActualStartDate, actualStartDate)
            .With(_ => _.PlannedEndDate, plannedEndDate)
            .With(_ => _.Uln, fixture.Create<long>().ToString)
            .Create();

        await _context.Get<TestMessageBus>().Publish(_apprenticeshipCreatedEvent);
    }

    [Then(@"Earnings results are published with calculated (.*), (.*), (.*), R(.*)-(.*), (.*)/(.*)")]
    public async Task ThenEarningsResultsArePublishedWithCalculated(decimal adjustedAgreedPrice, decimal learningAmount, int numberOfInstalments, short firstDeliveryPeriod, short firstDeliveryAcademicYear, short firstCalendarPeriodMonth, short firstCalendarPeriodYear)
    {
        await WaitHelper.WaitForIt(() => EarningsGeneratedEventHandler.ReceivedEvents.Any(), "Failed to find published event");

        _earnings = EarningsGeneratedEventHandler.ReceivedEvents.First();
        _earnings.FundingPeriods.Should().HaveAdjustedAgreedPriceOf(adjustedAgreedPrice);

        var firstFundingPeriod = _earnings.FundingPeriods.First();
        firstFundingPeriod.DeliveryPeriods.Should().HaveCount(numberOfInstalments);
        firstFundingPeriod.AgreedPrice.Should().Be(adjustedAgreedPrice);
        firstFundingPeriod.DeliveryPeriods.ForEach(dp => dp.LearningAmount.Should().Be(learningAmount));

        firstFundingPeriod.DeliveryPeriods.ShouldHaveCorrectFundingPeriods(numberOfInstalments, firstDeliveryPeriod, firstDeliveryAcademicYear);
        firstFundingPeriod.DeliveryPeriods.ShouldHaveCorrectFundingCalendarMonths(numberOfInstalments, firstCalendarPeriodMonth, firstCalendarPeriodYear);
    }

    [Then(@"correct Uln, EmployerId, ProviderId, TransferSenderEmployerId, StartDate, TrainingCode, EmployerType information")]
    public void ThenCorrectUlnEmployerIdProviderIdTransferSenderEmployerIdStartDateTrainingCodeEmployerTypeInformation()
    {
        _earnings.FundingPeriods.First().Uln.Should().Be(Convert.ToInt64(_apprenticeshipCreatedEvent.Uln));
        _earnings.FundingPeriods.First().EmployerId.Should().Be(_apprenticeshipCreatedEvent.EmployerAccountId);
        _earnings.FundingPeriods.First().ProviderId.Should().Be(_apprenticeshipCreatedEvent.UKPRN);
        _earnings.FundingPeriods.First().TransferSenderEmployerId.Should().Be(_apprenticeshipCreatedEvent.FundingEmployerAccountId);
        _earnings.FundingPeriods.First().StartDate.Should().Be(_apprenticeshipCreatedEvent.ActualStartDate);
        _earnings.FundingPeriods.First().TrainingCode.Should().Be(_apprenticeshipCreatedEvent.TrainingCode);
        _earnings.FundingPeriods.First().EmployerType.Should().Be(_apprenticeshipCreatedEvent.FundingType == FundingType.NonLevy ? EmployerType.NonLevy : EmployerType.Levy);
    }
}