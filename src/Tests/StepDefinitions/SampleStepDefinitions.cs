using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.MessageBus;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class SampleStepDefinitions
    {
        private readonly ScenarioContext _context;

        public SampleStepDefinitions(ScenarioContext context)
        {
            _context = context;
        }

        [Given(@"a sample event is published by our system")]
        public async Task GivenASampleEventIsPublishedByOurSystem()
        {
            var bus = _context.Get<TestMessageBus>();

            await bus.Publish(new SampleInputEvent { StartDate = new DateTime(2022, 8, 1)});
        }

        [Then(@"event data can be asserted on")]
        public async Task ThenEventDataCanBeAssertedOn()
        {
            await WaitHelper.WaitForIt(() => SampleEventHandler.ReceivedEvents.Any(), "Failed to find published Sample event");

            var @event = SampleEventHandler.ReceivedEvents.First();

            @event.Data.Should().Be("Hello Mike!");
        }
    }
}
