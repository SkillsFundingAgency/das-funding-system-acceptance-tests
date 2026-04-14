using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages.Events;

public class EarningsGeneratedEventHandler : MultipleEndpointSafeMessageHandler<EarningsGeneratedEvent> { }