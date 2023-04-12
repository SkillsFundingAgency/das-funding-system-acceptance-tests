using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

public class PaymentsGeneratedEventHandler : IHandleMessages<PaymentsGeneratedEvent>
{
    public static ConcurrentBag<PaymentsGeneratedEvent> ReceivedEvents { get; } = new();

    public Task Handle(PaymentsGeneratedEvent message, IMessageHandlerContext context)
    {
        ReceivedEvents.Add(message);
        return Task.CompletedTask;
    }
}
