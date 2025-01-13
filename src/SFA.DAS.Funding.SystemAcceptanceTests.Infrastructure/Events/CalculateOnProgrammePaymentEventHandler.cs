namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events;

public class CalculateOnProgrammePaymentEventHandler : BaseQueueReciever
{
    public static async Task<List<T>> ReceivedEvents<T>(Func<T, bool> predicate)
    {
        return await GetMatchingMessages(_fundingConfig.Pv2ServiceBusFqdn, _fundingConfig.Pv2FundingSourceQueue, predicate);
    }
}
