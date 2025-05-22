using SFA.DAS.Funding.ApprenticeshipPayments.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests;

internal class TestData
{
    internal string CurrentCollectionYear { get; set; }
    internal byte CurrentCollectionPeriod { get; set; }
    internal PaymentsGeneratedEvent PaymentsGeneratedEvent { get; set; }
}
