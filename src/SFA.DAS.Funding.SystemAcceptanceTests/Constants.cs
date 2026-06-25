namespace SFA.DAS.Funding.SystemAcceptanceTests;

internal static class Constants
{
    // TODO: Put these back as they were
    internal const long UkPrn = 88888392;// all tests use the same UKPRN
    internal const long AlternativeUkPrn = 88888393;// only to be used where required, e.g. change of provider
    internal const short WithdrawalReasonCode = 98;
    internal const short WithdrawalReasonCodeForRedundancy = 29;
}

internal static class UkprnProvider 
{
    internal static long GetUkprnForProvider(string provider)
    {
        return provider switch
        {
            "A" => Constants.UkPrn,
            "B" => Constants.AlternativeUkPrn,
            _ => throw new ArgumentException($"Invalid training provider - {provider}", nameof(provider))
        };
    }
}