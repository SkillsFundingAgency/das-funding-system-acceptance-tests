using System.Collections.Concurrent;
using Newtonsoft.Json;
using NServiceBus;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

#pragma warning disable CS8620 // Ingore nullability warning for generic type T, as this is a base class for event handlers that can handle any type of message.
public class MultipleEndpointSafeEventHandler<T> : IHandleMessages<T> where T : class, new()
{   
    private static ConcurrentBag<MessageWrapper<T>> _receivedEvents = new();

    public static IEnumerable<MessageWrapper<T>> ReceivedEvents
    {
        get
        {
            return _receivedEvents.Where(x=> !x.IsCleared).ToList(); // Defensive copy
        }
    }

    /// <summary>
    /// Gets the first message that matches the predicate and removes it from the list of received events.
    /// </summary>
    public static T? GetMessage(Func<T, bool> predicate)
    {
        var eventObjects = ReceivedEvents.Where(x => predicate(x.Message));

        if (eventObjects == null || !eventObjects.Any())
            return null;

        if(eventObjects.Count() > 1)
        {
            throw new InvalidOperationException($"Multiple {nameof(T)} messages found matching the predicate. Expected only one, but found {eventObjects.Count()}.");
        }

        var eventObject = eventObjects.Single();

        eventObject.Clear(); // Prevent message getting picked up again
        return eventObject.Message;
    }

    public Task Handle(T message, IMessageHandlerContext context)
    {
        var json = JsonConvert.SerializeObject(message);
        var obj = JsonConvert.DeserializeObject<T>(json);

        _receivedEvents.Add(new MessageWrapper<T>(context.MessageId, obj));

        return Task.CompletedTask;
    }

    public static void Clear(Func<T, bool> predicate)
    {
        var filtered = _receivedEvents.Where(e => predicate(e.Message));
        foreach (var item in filtered)
        {
            item.Clear();
        }
    }
}

public class MessageWrapper<T>
{
    public T Message { get; private set; }
    public string MessageId { get; private set; }
    public bool IsCleared { get; private set; } = false;
    public MessageWrapper(string messageId, T message)
    {
        Message = message;
        MessageId = messageId;
    }
    public void Clear()
    {
        IsCleared = true;
    }
}
#pragma warning restore CS8620 // Ingore nullability warning for generic type T, as this is a base class for event handlers that can handle any type of message.