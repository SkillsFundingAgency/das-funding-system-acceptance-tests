using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

public class EarningsGeneratedEventHandler : MultipleEndpointSafeEventHandler<EarningsGeneratedEvent> { }