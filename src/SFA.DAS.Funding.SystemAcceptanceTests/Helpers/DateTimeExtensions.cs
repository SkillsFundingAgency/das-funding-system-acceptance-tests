namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

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
}
