using NServiceBus;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings settings)
        {
            settings.RouteToEndpoint(typeof(ApprenticeshipCreatedEvent), QueueNames.ApprenticeshipLearners);
            settings.RouteToEndpoint(typeof(EarningsGeneratedEvent), QueueNames.EarningsGenerated);
        }
    }
}
