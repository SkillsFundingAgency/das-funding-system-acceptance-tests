using NServiceBus;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events
{
    public class CalculatedRequiredLevyAmountEventHandler : IHandleMessages<CalculatedRequiredLevyAmount>
    {
        public static ConcurrentBag<CalculatedRequiredLevyAmount> ReceivedEvents { get; } = new();

        public Task Handle(CalculatedRequiredLevyAmount message, IMessageHandlerContext context)
        {
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }
    }
}
