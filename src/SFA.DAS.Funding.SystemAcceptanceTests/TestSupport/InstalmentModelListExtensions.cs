namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public static class InstalmentModelListExtensions
{
    /// <summary>
    /// Asserts a condition for all instalments between two periods inclusive.
    /// </summary>
    /// <param name="instalmentsList">The list of instalment models (e.g. from the earnings profile that has been pulled from the DB)</param>
    /// <param name="firstPeriod">Starting period (inclusive)</param>
    /// <param name="secondPeriod">Ending period (inclusive)</param>
    /// <param name="assertion">The assertion to perform on ALL instalments between the two inclusive periods</param>
    /// <param name="failureText">The function to construct the failure text for a given instalment which does not match the assertion predicate</param>
    /// <exception cref="ArgumentException"></exception>
    public static void AssertBetweenRange(this List<InstalmentModel> instalmentsList, Period firstPeriod, Period secondPeriod, Func<InstalmentModel, bool> assertion, Func<InstalmentModel, string> failureText)
    {
        if (secondPeriod.IsBefore(firstPeriod))
            throw new ArgumentException($"Second period must be after first period for asserting a condition between a range of instalments.");

        var currentAy = firstPeriod.AcademicYear;
        var currentDp = firstPeriod.PeriodValue;
        var endingAy = secondPeriod.AcademicYear;
        var endingDp = secondPeriod.PeriodValue;

        while (true)
        {
            instalmentsList
                .Where(x => x.AcademicYear == currentAy && x.DeliveryPeriod == currentDp)
                .ToList()
                .ForEach(i =>
                {
                    Assert.IsTrue(assertion.Invoke(i), failureText.Invoke(i));
                });

            if (currentAy == endingAy && currentDp == endingDp)
            {
                break;
            }

            currentDp++;
            if (currentDp > 12)
            {
                currentDp = 1;
                currentAy++;
            }
        }
    }
}