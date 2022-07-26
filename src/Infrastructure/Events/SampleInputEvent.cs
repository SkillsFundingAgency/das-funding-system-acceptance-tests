using NServiceBus;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events
{
    public class SampleInputEvent : IEvent
    {
        public DateTime StartDate { get; set; }
    }
}
