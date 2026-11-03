namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;

public static class DateTimeExtensions
{
    public static DateTime GetYearMonthFirstDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static (short AcademicYear, byte Period) ToAcademicYearAndPeriod(this DateTime date)
    {
        var twoDigitCalendarYear = short.Parse(date.Year.ToString().Substring(2, 2));

        return date.Month < 8
            ? (short.Parse($"{twoDigitCalendarYear - 1}{twoDigitCalendarYear}"), (byte)(date.Month + 5))
            : (short.Parse($"{twoDigitCalendarYear}{twoDigitCalendarYear + 1}"), (byte)(date.Month - 7));
    }

    public static DateOnly? ToDateOnly(this DateTime? value) => value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
    public static DateOnly ToDateOnly(this DateTime value) => DateOnly.FromDateTime(value);

    public static int GetNumberOfIncludedCensusDatesUntil(this DateTime start, DateTime end)
    {
        var totalDays = start.GetNumberOfDaysUntil(end);
        var includedCensusDateCounter = 0;
        for (var i = 0; i < totalDays; i++)
        {
            if (start.AddDays(i).Day == DateTime.DaysInMonth(start.AddDays(i).Year, start.AddDays(i).Month))
                includedCensusDateCounter++;
        }
        return includedCensusDateCounter;
    }

    public static int GetNumberOfDaysUntil(this DateTime start, DateTime end)
    {
        var nonInclusiveDays = (end - start).Days;

        if (nonInclusiveDays < 0)
            return 0;

        return 1 + nonInclusiveDays;
    }
}
