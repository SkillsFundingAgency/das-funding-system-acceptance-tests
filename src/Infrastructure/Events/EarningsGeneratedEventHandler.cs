using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events
{
    public class EarningsGeneratedEventHandler : IHandleMessages<EarningsGeneratedEvent>
    {
        public static ConcurrentBag<EarningsGeneratedEvent> ReceivedEvents { get; } = new();

        public Task Handle(EarningsGeneratedEvent message, IMessageHandlerContext context)
        {
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }

    }
}
