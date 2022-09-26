using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events
{
    public  class ApprenticeshipsTypesEventHandler : IHandleMessages<ApprenticeshipCreatedEvent>
    {
        public static ConcurrentBag<ApprenticeshipCreatedEvent> ReceivedEvents { get; } = new();

        public Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
        {
            ReceivedEvents.Add(message);
            return Task.CompletedTask;
        }
    }
}
