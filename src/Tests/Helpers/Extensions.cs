namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class Extensions
{
    public static void ShouldHaveCorrectFundingPeriods(this List<DeliveryPeriod> periods, int numberOfInstalments, short firstDeliveryPeriod, short firstDeliveryAcademicYear)
    {
        if (firstDeliveryPeriod > 12) throw new ArgumentException("Period can't be greater than 12 can it?", nameof(firstDeliveryPeriod));

        var expectedPeriod = firstDeliveryPeriod;
        var expectedAcademicYear = firstDeliveryAcademicYear;

        List<(short Period, short AcademicYear)> expected = new();

        for (var i = 0; i < numberOfInstalments; i++)
        {
            expected.Add(new ValueTuple<short, short>(expectedPeriod, expectedAcademicYear));
            if (expectedPeriod == 12)
            {
                expectedPeriod = 1;
                expectedAcademicYear = GetNextAcademicYear(expectedAcademicYear);
            }
            else expectedPeriod++;
        }

        var actual  = periods
            .Select(c => new { c.Period, c.AcademicYear})
            .AsEnumerable()
            .Select(c => new Tuple<short, short>(c.Period, c.AcademicYear))
            .ToList();

        actual.Should().BeEquivalentTo(expected);
    }

    private static short GetNextAcademicYear(short expectedAcademicYear)
    {
        var firstPart = expectedAcademicYear / 100 + 1;
        var secondPart = expectedAcademicYear % 100 + 1;
        return (short)(firstPart * 100 + secondPart);
    }

    public static void ShouldHaveCorrectFundingCalendarMonths(this List<DeliveryPeriod> periods, int numberOfInstalments, short firstCalendarPeriodMonth, short firstCalendarPeriodYear)
    {
        if (firstCalendarPeriodMonth > 12) throw new ArgumentException("A Month number can't be greater than 12 can it?", nameof(firstCalendarPeriodMonth));

        var expectedMonth = firstCalendarPeriodMonth;
        var expectedYear = firstCalendarPeriodYear;

        List<(short CalendarMonth, short CalenderYear)> expected = new();

        for (var i = 0; i < numberOfInstalments; i++)
        {
            expected.Add(new ValueTuple<short, short>(expectedMonth, expectedYear));
            if (expectedMonth == 12)
            {
                expectedMonth = 1;
                expectedYear++;
            }
            else expectedMonth++;
        }

        var actual = periods
            .Select(c => new { c.CalendarMonth, c.CalenderYear })
            .AsEnumerable()
            .Select(c => new Tuple<short, short>(c.CalendarMonth, c.CalenderYear))
            .ToList();

        actual.Should().BeEquivalentTo(expected);
    }
}