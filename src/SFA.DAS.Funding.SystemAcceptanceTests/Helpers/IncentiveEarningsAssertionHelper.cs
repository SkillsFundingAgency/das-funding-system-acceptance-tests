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
    /// <param name="dueDate">The date incentive is expected to be due (optional)</param>
    /// <param name="breakInLearning">The date ranges for Break In Learnings of the episode (optional)</param>
    /// <returns>True if a matching incentive earning exists; otherwise, false.</returns>
    public static bool IncentiveEarningExists(this List<AdditionalPaymentsModel> additionalPayments, DateTime startDate, byte incentiveNumber,
        AdditionalPaymentType paymentType, DateTime? dueDate = null, List<EpisodeBreakInLearning>? breakInLearning = null)
    {
        if (incentiveNumber is not (1 or 2))
            throw new Exception("Assertion helper only supports incentive earning number of 1 or 2");

        var expectedPeriod = incentiveNumber == 2
            ? AdjustForBreaks(startDate, 364, breakInLearning).ToAcademicYearAndPeriod() // 365th day of learning
            : AdjustForBreaks(startDate, 89, breakInLearning).ToAcademicYearAndPeriod(); // 90th day of learning

        return additionalPayments.Any(x =>
            x.AcademicYear == expectedPeriod.AcademicYear
            && x.DeliveryPeriod == expectedPeriod.Period
            && x.AdditionalPaymentType == paymentType
            && x.Amount == 500
            && x.IsAfterLearningEnded == false
            && (dueDate == null || x.DueDate == dueDate));
    }

    private static DateTime AdjustForBreaks(DateTime startDate, int milestoneDays, List<EpisodeBreakInLearning> breaksInLearning)
    {
        var incentiveDate = startDate.AddDays(milestoneDays);

        if (breaksInLearning == null || breaksInLearning.Count == 0)
            return incentiveDate;

        foreach (var b in breaksInLearning.OrderBy(x => x.StartDate))
        {
            int duration = (b.EndDate - b.StartDate).Days + 1;

            if (b.StartDate <= incentiveDate)
            {
                incentiveDate = incentiveDate.AddDays(duration);
            }
        }

        return incentiveDate;
    }
}