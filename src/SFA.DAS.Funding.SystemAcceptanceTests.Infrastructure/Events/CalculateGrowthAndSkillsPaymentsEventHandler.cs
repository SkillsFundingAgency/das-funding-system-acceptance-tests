using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events
{
    public class CalculateGrowthAndSkillsPaymentsEventHandler : MultipleEndpointSafeEventHandler<CalculateGrowthAndSkillsPayments> { }
}
