using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using WireMock.Server;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

/// <summary>
/// This class is used to hold static objects that are shared across the test run.
/// </summary>
internal class StaticObjects
{
    internal static WireMockServer? WireMockServer;
    internal static LearningClient? ApprenticeshipsClient;
    internal static EarningsOuterClient? EarningsOuterClient;
    internal static LearningSqlClient? ApprenticeshipsSqlClient;
    internal static EarningsSqlClient? EarningsSqlClient;
    internal static ApprenticeshipsInnerApiHelper? ApprenticeshipsInnerApiHelper;
    internal static EarningsInnerApiHelper? EarningsInnerApiHelper;
    internal static LearnerDataOuterApiHelper? LearnerDataOuterApiHelper;
}
