using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    internal class ServiceBusMessageHelper
    {
        private readonly ScenarioContext _context;

        public ServiceBusMessageHelper(ScenarioContext context)
        {
            _context = context;
        }

        public ApprenticeshipCreatedEvent CreateApprenticeshipCreatedMessageWithCustomValues(DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice)
        {
            var fixture = new Fixture();
             return fixture.Build<ApprenticeshipCreatedEvent>()
                .With(_ => _.ActualStartDate, actualStartDate)
                .With(_ => _.PlannedEndDate, plannedEndDate)
                .With(_ => _.AgreedPrice, agreedPrice)
                .With(_ => _.FundingBandMaximum, agreedPrice)
                .With(_ => _.Uln, fixture.Create<long>().ToString)
                .Create();
        }

        public ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithFundingBandMaximumValue(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, decimal fundingBandMax)
        {
            apprenticeshipCreatedEvent.FundingBandMaximum = fundingBandMax;

            return apprenticeshipCreatedEvent;
        }

        public async Task PublishApprenticeshipApprovedMessage(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            await _context.Get<TestMessageBus>().Publish(apprenticeshipCreatedEvent);
            await WaitHelper.WaitForIt(() => EarningsGeneratedEventHandler.ReceivedEvents.Where(x => x.ApprenticeshipKey == apprenticeshipCreatedEvent.ApprenticeshipKey).Any(), "Failed to find published event");
        }

        public EarningsGeneratedEvent ReadEarningsGeneratedMessage(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            return EarningsGeneratedEventHandler.ReceivedEvents.Where(x => x.ApprenticeshipKey == apprenticeshipCreatedEvent.ApprenticeshipKey).First();
        }
    }
}