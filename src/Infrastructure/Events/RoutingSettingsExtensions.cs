using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using QueueNames = SFA.DAS.Funding.ApprenticeshipEarnings.Types.QueueNames;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

public static class RoutingSettingsExtensions
{
    public static void AddRouting(this RoutingSettings settings)
    {
       // settings.RouteToEndpoint(typeof(ApprenticeshipCreatedEvent), QueueNames.ApprenticeshipCreated);
       // settings.RouteToEndpoint(typeof(EarningsGeneratedEvent), QueueNames.EarningsGenerated);
    }
}