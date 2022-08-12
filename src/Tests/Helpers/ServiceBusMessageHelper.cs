namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    internal class ServiceBusMessageHelper
    {
        private readonly ScenarioContext _context;
        private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
        private EarningsGeneratedEvent _earnings;

        public ServiceBusMessageHelper(ScenarioContext context)
        {
            _context = context;
        }

        public void CreateAnApprenticeshipMessage(DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice)
        {
            var fixture = new Fixture();
            _apprenticeshipCreatedEvent = fixture.Build<ApprenticeshipCreatedEvent>()
                .With(_ => _.ActualStartDate, actualStartDate)
                .With(_ => _.PlannedEndDate, plannedEndDate)
                .With(_ => _.AgreedPrice, agreedPrice)
                .With(_ => _.Uln, fixture.Create<long>().ToString)
                .Create();

            _context.Set(_apprenticeshipCreatedEvent);
        }

        public async Task PublishAnApprenticeshipApprovedMessage()
        {
            await _context.Get<TestMessageBus>().Publish(_apprenticeshipCreatedEvent);
        }

        public async Task ReadEarningsGeneratedMessage()
        {
            await WaitHelper.WaitForIt(() => EarningsGeneratedEventHandler.ReceivedEvents.Where(x => x.ApprenticeshipKey == _apprenticeshipCreatedEvent.ApprenticeshipKey).Any(), "Failed to find published event");

            _earnings = EarningsGeneratedEventHandler.ReceivedEvents.First();

            _context.Set(_earnings);
        }
    }
}