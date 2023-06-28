using System.Collections.Concurrent;
using NServiceBus;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

public class MultipleEndpointSafeEventHandler<T> : IHandleMessages<T>
{
    public static ConcurrentBag<(IMessageHandlerContext context, T message)> ReceivedEvents { get; set; } = new();

    public Task Handle(T message, IMessageHandlerContext context)
    {
        ReceivedEvents.Add((context, message));
        ReceivedEvents = new ConcurrentBag<(IMessageHandlerContext context, T message)>(ReceivedEvents.GroupBy(x => x.context.MessageId).Select(g => g.First()));
        return Task.CompletedTask;
    }
}