namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class DateTimeExtensions
{
    public static DateTime GetYearMonthFirstDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }
}
