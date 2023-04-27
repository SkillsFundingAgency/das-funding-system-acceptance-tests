using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

public class FinalisedOnProgrammeLearningPaymentEventHandler: IHandleMessages<FinalisedOnProgammeLearningPaymentEvent>
{
    public static ConcurrentBag<FinalisedOnProgammeLearningPaymentEvent> ReceivedEvents { get; } = new();

    public Task Handle(FinalisedOnProgammeLearningPaymentEvent message, IMessageHandlerContext context)
    {
        ReceivedEvents.Add(message);
        return Task.CompletedTask;
    }
}
