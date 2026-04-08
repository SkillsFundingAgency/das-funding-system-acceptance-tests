using SFA.DAS.LearnerData.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages.Events;

public class LearnerDataEventHandler : MultipleEndpointSafeMessageHandler<LearnerDataEvent> { }
