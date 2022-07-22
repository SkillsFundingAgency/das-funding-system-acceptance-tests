using AutoFixture;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Funding.IntegrationTests.Helpers;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.Events;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.MessageBus;

namespace SFA.DAS.Funding.IntegrationTests.StepDefinitions
{
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

        [Given(@"An apprenticeship is created with (.*), (.*)(.*), (.*)(.*)")]
        public async Task GivenAnApprenticeshipIsCreatedWith(decimal agreedPrice, DateTime actualStartDate, DateTime plannedEndDate)
        {
            _apprenticeshipCreatedEvent = new Fixture().Build<ApprenticeshipCreatedEvent>()
                .With(_ => _.AgreedPrice, agreedPrice)
                .With(_ => _.ActualStartDate, actualStartDate)
                .With(_ => _.PlannedEndDate, plannedEndDate)
                .Create();

            await _context.Get<TestMessageBus>().Publish(_apprenticeshipCreatedEvent);
        }

        [Then(@"Earnings results are published with calculated (.*), (.*), (.*), R(.*), (.*)")]
        public async Task ThenEarningsResultsArePublishedWithCalculated(decimal adjustedAgreedPrice, decimal learningAmount, int numberOfInstalments, string firstDeliveryPeriod, string firstCalendarPeriod)
        {
            await WaitHelper.WaitForIt(() => EarningsGeneratedEventHandler.ReceivedEvents.Any(), "Failed to find published event");

            _earnings = EarningsGeneratedEventHandler.ReceivedEvents.First();

            var firstFundingPeriod = _earnings.FundingPeriods.First();
            firstFundingPeriod.AgreedPrice.Should().Be(adjustedAgreedPrice);
            firstFundingPeriod.DeliveryPeriods.ForEach(dp => dp.LearningAmount.Should().Be(learningAmount));
            firstFundingPeriod.DeliveryPeriods.Should().HaveCount(numberOfInstalments);

            // TODO verify periods
        }

        [Then(@"correct Uln, EmployerId, ProviderId, TransferSenderEmployerId, StartDate, TrainingCode, EmployerType information")]
        public void ThenCorrectUlnEmployerIdProviderIdTransferSenderEmployerIdStartDateTrainingCodeEmployerTypeInformation()
        {
            _earnings.FundingPeriods.First().Uln.Should().Be(_apprenticeshipCreatedEvent.Uln);
            // etc.
        }
    }
}
