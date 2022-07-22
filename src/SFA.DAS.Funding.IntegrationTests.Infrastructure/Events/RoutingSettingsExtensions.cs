using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.IntegrationTests.Infrastructure.Events
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings settings)
        {
            settings.RouteToEndpoint(typeof(SampleOutputEvent), QueueNames.EarningsGenerated);
        }
    }
}
