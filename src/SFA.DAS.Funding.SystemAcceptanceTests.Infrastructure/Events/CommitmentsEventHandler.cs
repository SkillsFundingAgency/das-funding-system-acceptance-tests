using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events
{
    public class CommitmentsEventHandler : IHandleMessages<ApprenticeshipCreatedEvent>
    {
        public static ConcurrentBag<ApprenticeshipCreatedEvent> ReceivedEvents { get; } = new();

        public Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }
    }
}
