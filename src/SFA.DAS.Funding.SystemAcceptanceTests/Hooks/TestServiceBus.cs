using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks
{
    public static class TestServiceBus
    {
        public static FundingConfig Config { get; set; } = null!;
        public static TestMessageBus Das { get; set; } = null!;
        public static TestMessageBus Pv2 { get; set; } = null!;
    }
}
