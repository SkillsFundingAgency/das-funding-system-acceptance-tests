using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class IncentiveEarningsAssertionHelper
{
    /// <summary>
    /// This checks if an incentive earning exists in the given list of additional payment models
    /// </summary>
    /// <param name="additionalPayments">The list of additional payment models to search.</param>
    /// <param name="startDate">The start date of the apprenticeship.</param>
    /// <param name="incentiveNumber">The incentive number (1 or 2)</param>
    /// <param name="paymentType">The type of incentive to look for (ProviderIncentive or EmployerIncentive).</param>
    /// <returns>True if a matching incentive earning exists; otherwise, false.</returns>
    public static bool IncentiveEarningExists(this List<AdditionalPaymentsModel> additionalPayments, DateTime startDate, byte incentiveNumber, AdditionalPaymentType paymentType)
    {
        if (incentiveNumber is not (1 or 2))
            throw new Exception("Assertion helper only supports incentive earning number of 1 or 2");

        var expectedPeriod = incentiveNumber == 2
            ? startDate.AddDays(364).ToAcademicYearAndPeriod() // 365th day of learning
            : startDate.AddDays(89).ToAcademicYearAndPeriod(); // 90th day of learning

        return additionalPayments.Any(x =>
            x.AcademicYear == expectedPeriod.AcademicYear
            && x.DeliveryPeriod == expectedPeriod.Period
            && x.AdditionalPaymentType == paymentType
            && x.Amount == 500);
    }
}