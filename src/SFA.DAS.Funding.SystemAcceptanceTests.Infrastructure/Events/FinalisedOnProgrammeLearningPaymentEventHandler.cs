using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using System.Collections.Concurrent;
using SpecFlow.Internal.Json;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

public class FinalisedOnProgrammeLearningPaymentEventHandler: IHandleMessages<FinalisedOnProgammeLearningPaymentEvent>
{
    public static ConcurrentBag<(IMessageHandlerContext context, FinalisedOnProgammeLearningPaymentEvent message)> ReceivedEvents { get; set; } = new();

    public Task Handle(FinalisedOnProgammeLearningPaymentEvent message, IMessageHandlerContext context)
    {
        ReceivedEvents.Add((context, message));
        ReceivedEvents = new ConcurrentBag<(IMessageHandlerContext context, FinalisedOnProgammeLearningPaymentEvent message)>(ReceivedEvents.GroupBy(x => x.context.MessageId).Select(g => g.First()));
        return Task.CompletedTask;
    }
}
