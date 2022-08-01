using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.MessageBus;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class CalculateEarningsForLearningPaymentsStepDefinitions
    {
        private readonly ScenarioContext _context;
        private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
        private EarningsGeneratedEvent _earnings;
        private DateTime _actualStartDate;

        public CalculateEarningsForLearningPaymentsStepDefinitions(ScenarioContext context)
        {
            _context = context;
        }

        [Given(@"An apprenticeship is created with (.*), (.*), (.*)")]
        public async Task GivenAnApprenticeshipIsCreatedWith(decimal agreedPrice, DateTime actualStartDate, DateTime plannedEndDate)
        {
            _actualStartDate = actualStartDate;
            _apprenticeshipCreatedEvent = new Fixture().Build<ApprenticeshipCreatedEvent>()
                .With(_ => _.AgreedPrice, agreedPrice)
                .With(_ => _.ActualStartDate, actualStartDate)
                .With(_ => _.PlannedEndDate, plannedEndDate)
                .Create();

            try
            {
                await _context.Get<TestMessageBus>().Publish(_apprenticeshipCreatedEvent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail(e.Message);
            }
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

            firstFundingPeriod.DeliveryPeriods.ShouldHaveCorrectFundingPeriods(_actualStartDate, numberOfInstalments, firstDeliveryPeriod, firstDeliveryAcademicYear);
            firstFundingPeriod.DeliveryPeriods.ShouldHaveCorrectFundingCalendarMonths(_actualStartDate, numberOfInstalments, firstCalendarPeriodMonth, firstCalendarPeriodYear);
        }

        [Then(@"correct Uln, EmployerId, ProviderId, TransferSenderEmployerId, StartDate, TrainingCode, EmployerType information")]
        public void ThenCorrectUlnEmployerIdProviderIdTransferSenderEmployerIdStartDateTrainingCodeEmployerTypeInformation()
        {
            _earnings.FundingPeriods.First().Uln.Should().Be(_apprenticeshipCreatedEvent.Uln);
            _earnings.FundingPeriods.First().EmployerId.Should().Be(_apprenticeshipCreatedEvent.EmployerAccountId);
            _earnings.FundingPeriods.First().ProviderId.Should().Be(_apprenticeshipCreatedEvent.UKPRN);
            _earnings.FundingPeriods.First().TransferSenderEmployerId.Should().Be(_apprenticeshipCreatedEvent.FundingEmployerAccountId);
            _earnings.FundingPeriods.First().StartDate.Should().Be(_apprenticeshipCreatedEvent.ActualStartDate);
            _earnings.FundingPeriods.First().TrainingCode.Should().Be(_apprenticeshipCreatedEvent.TrainingCode);
            _earnings.FundingPeriods.First().EmployerType.Should().Be(_apprenticeshipCreatedEvent.FundingType == FundingType.NonLevy ? EmployerType.NonLevy : EmployerType.Levy);
        }
    }


    public static class Extensions // todo: move to fluent assertions extensions
    {
        public static void ShouldHaveCorrectFundingPeriods(this List<DeliveryPeriod> periods, DateTime startDate, int numberOfInstalments, short firstDeliveryPeriod, short firstDeliveryAcademicYear)
        {
            if (firstDeliveryPeriod > 12) throw new ArgumentException("Period can't be greater than 12 can it?", nameof(firstDeliveryPeriod));

            var expectedPeriod = firstDeliveryPeriod;
            var expectedAcademicYear = firstDeliveryAcademicYear;

            List<(short Period, short AcademicYear)> expected = new();

            for (var i = 0; i < numberOfInstalments; i++)
            {
                expected.Add(new ValueTuple<short, short>(expectedPeriod, expectedAcademicYear));
                if (expectedPeriod == 12)
                {
                    expectedPeriod = 1;
                    expectedAcademicYear = GetNextAcademicYear(expectedAcademicYear);
                }
                else
                {
                    expectedPeriod++;
                }
            }

            var actual  = periods
                .Select(c => new { c.Period, c.AcademicYear})
                .AsEnumerable()
                .Select(c => new Tuple<short, short>(c.Period, c.AcademicYear))
                .ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        private static short GetNextAcademicYear(short expectedAcademicYear)
        {
            var firstPart = expectedAcademicYear / 100 + 1;
            var secondPart = expectedAcademicYear % 100 + 1;
            return (short)(firstPart * 100 + secondPart);
        }

        public static void ShouldHaveCorrectFundingCalendarMonths(this List<DeliveryPeriod> periods, DateTime startDate, int numberOfInstalments, short firstCalendarPeriodMonth, short firstCalendarPeriodYear)
        {
            if (firstCalendarPeriodMonth > 12) throw new ArgumentException("A Month number can't be greater than 12 can it?", nameof(firstCalendarPeriodMonth));

            var expectedMonth = firstCalendarPeriodMonth;
            var expectedYear = firstCalendarPeriodYear;

            List<(short CalendarMonth, short CalenderYear)> expected = new();

            for (var i = 0; i < numberOfInstalments; i++)
            {
                expected.Add(new ValueTuple<short, short>(expectedMonth, expectedYear));
                if (expectedMonth == 12)
                {
                    expectedMonth = 1;
                    expectedYear++;
                }
                else
                {
                    expectedMonth++;
                }
            }

            var actual = periods
                .Select(c => new { c.CalendarMonth, c.CalenderYear })
                .AsEnumerable()
                .Select(c => new Tuple<short, short>(c.CalendarMonth, c.CalenderYear))
                .ToList();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
