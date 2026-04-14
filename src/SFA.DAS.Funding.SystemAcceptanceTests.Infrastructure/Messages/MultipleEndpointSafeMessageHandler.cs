using System.Collections.Concurrent;
using Newtonsoft.Json;
using NServiceBus;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages;

#pragma warning disable CS8620 // Ignore nullability warning for generic type T, as this is a base class for message handlers that can handle any type of message.
public class MultipleEndpointSafeMessageHandler<T> : IHandleMessages<T> where T : class, new()
{   
    private static ConcurrentBag<MessageWrapper<T>> _receivedMessages = new();

    public static IEnumerable<MessageWrapper<T>> ReceivedMessages
    {
        get
        {
            return _receivedMessages.Where(x=> !x.IsCleared).ToList(); // Defensive copy
        }
    }

    /// <summary>
    /// Gets the first message that matches the predicate and removes it from the list of received events.
    /// </summary>
    public static T? GetMessage(Func<T, bool> predicate)
    {
        var messageObjects = ReceivedMessages.Where(m => predicate(m.Message));

        if (messageObjects == null || !messageObjects.Any())
            return null;

        var messageObject = messageObjects.First();

        messageObject.Clear(); // Prevent message getting picked up again
        return messageObject.Message;
    }

    public Task Handle(T message, IMessageHandlerContext context)
    {
        var json = JsonConvert.SerializeObject(message);
        var obj = JsonConvert.DeserializeObject<T>(json);

        _receivedMessages.Add(new MessageWrapper<T>(context.MessageId, obj));

        return Task.CompletedTask;
    }

    public static void Clear(Func<T, bool> predicate)
    {
        var filtered = _receivedMessages.Where(m => predicate(m.Message));
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
#pragma warning restore CS8620 // Ignore nullability warning for generic type T, as this is a base class for message handlers that can handle any type of message.