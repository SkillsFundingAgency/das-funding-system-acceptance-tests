using System.Collections.Concurrent;
using NServiceBus;

namespace SFA.DAS.Funding.IntegrationTests.Infrastructure.Events
{
    public class SampleEventHandler : IHandleMessages<SampleOutputEvent>
    {
        public static ConcurrentBag<SampleOutputEvent> ReceivedEvents { get; } = new();

        public Task Handle(SampleOutputEvent message, IMessageHandlerContext context)
        {
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }

    }
}
