using System.Collections.Concurrent;
using Newtonsoft.Json;
using NServiceBus;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

#pragma warning disable CS8620 // Ingore nullability warning for generic type T, as this is a base class for event handlers that can handle any type of message.
public class MultipleEndpointSafeEventHandler<T> : IHandleMessages<T>
{
    public static ConcurrentBag<(string messageId, T message)> ReceivedEvents { get; set; } = new();

    public Task Handle(T message, IMessageHandlerContext context)
    {
        var json = JsonConvert.SerializeObject(message);
        var obj = JsonConvert.DeserializeObject<T>(json);
        ReceivedEvents.Add(($"{context.MessageId}", obj));

        ReceivedEvents = new ConcurrentBag<(string messageId, T message)>(ReceivedEvents.GroupBy(x => x.messageId).Select(g => g.First()));
        return Task.CompletedTask;
    }
}
#pragma warning restore CS8620 // Ingore nullability warning for generic type T, as this is a base class for event handlers that can handle any type of message.