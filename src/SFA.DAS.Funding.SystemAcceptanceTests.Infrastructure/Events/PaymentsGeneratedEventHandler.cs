using SFA.DAS.Funding.ApprenticeshipPayments.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

public class PaymentsGeneratedEventHandler : MultipleEndpointSafeEventHandler<PaymentsGeneratedEvent> { }