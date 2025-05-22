using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests;

internal class TestData
{
    internal Guid ApprenticeshipKey { get; set; } = Guid.Empty;
    internal string CurrentCollectionYear { get; set; }
    internal byte CurrentCollectionPeriod { get; set; }
    internal PaymentsGeneratedEvent PaymentsGeneratedEvent { get; set; }
    internal EarningsApprenticeshipModel? EarningsApprenticeshipModel { get; set; }
    internal CommitmentsMessages.ApprenticeshipCreatedEvent CommitmentsApprenticeshipCreatedEvent { get; set; }
    internal Apprenticeships.Types.ApprenticeshipCreatedEvent ApprenticeshipCreatedEvent { get; set; }
    internal EarningsGeneratedEvent EarningsGeneratedEvent { get; set; }
}
