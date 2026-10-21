namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public enum WithdrawalReason : short
{
    TransferredToAnotherProvider = 2,
    LearnerInjuryOrIllness = 3,
    TransferredBetweenProvidersByDfeAgreement = 7,
    MadeRedundant = 29,
    TransferredToNewLearningAimSameProvider = 40,
    TransferredToAnotherProviderGovernmentStrategy = 41,
    AcademicFailureOrBadStanding = 42,
    FinancialReasons = 43,
    OtherPersonalReasons = 44,
    WrittenOffAfterLapseOfTime = 45,
    Exclusion = 46,
    TransferredToAnotherProviderDueToMerger = 47,
    IndustryPlacementWithdrawnOutsideProviderControl = 48,
    Other = 97,
    ReasonNotKnown = 98
}

public static class WithdrawalReasonExtensions
{
    public static WithdrawalReason ToWithdrawalReason(this short value)
    {
        if (!Enum.IsDefined(typeof(WithdrawalReason), value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown withdrawal reason code.");
        }

        return (WithdrawalReason)value;
    }

    public static short ToWithdrawalReasonCode(this string value)
    {
        if (short.TryParse(value, out var numericCode))
        {
            return numericCode.ToWithdrawalReason().ToCode();
        }

        if (Enum.TryParse<WithdrawalReason>(value, true, out var enumValue))
        {
            return enumValue.ToCode();
        }

        throw new ArgumentException($"Unknown withdrawal reason '{value}'", nameof(value));
    }

    public static short ToCode(this WithdrawalReason value)
    {
        return (short)value;
    }
}
