using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class TableExtensions
{
    public static List<(byte Period, short AcademicYear, byte Month)> ToExpectedPeriods(this Table table)
    {
        return table.Rows.Select(row => 
                (Convert.ToByte(row[0]), Convert.ToInt16(row[1]), Months[row[2]]))
            .ToList();
    }

    public static List<(short AcademicYear, byte DeliveryPeriod, decimal Amount, short CollectionYear, byte CollectionPeriod)> ToExpectedPayments(this Table table)
    {
        return table.Rows.Select(row =>
                (Convert.ToInt16(row[0]), Period[row[1]], Convert.ToDecimal(row[2]), Convert.ToInt16(row[3]), Period[row[4]]))
            .ToList();
    }

    public static List<(short AcademicYear, byte DeliveryPeriod, decimal Amount, short CollectionYear, byte CollectionPeriod)> ToExpectedRollupPayments(this Table table)
    {
        return table.Rows.Select(row =>
                (Convert.ToInt16(CalculateAcademicYear(row[0])), Period[CalculatePeriod(row[0])], Convert.ToDecimal(row[2]),
                Convert.ToInt16(CalculateAcademicYear(row[1])), Period[CalculatePeriod(row[1])])).ToList();
    }

    private static string CalculatePeriod(string text)
    {
        var numberOfMonthsToAdd = new string(text.Where(c => !char.IsLetter(c)).ToArray());

        return DateTime.Now.AddMonths(Convert.ToInt16(numberOfMonthsToAdd)).ToString("MMMM");
    }

    public static string CalculateAcademicYear(string numberOfMonthsToAdd, DateTime date = default)
    {
        var numberOfMonths = new string(numberOfMonthsToAdd.Where(c => !char.IsLetter(c)).ToArray());

        if (date == default) date = DateTime.Now;

        var month = date.AddMonths(Convert.ToInt16(numberOfMonths)).Month;
        var Year = date.AddMonths(Convert.ToInt16(numberOfMonths)).Year;

        if (month < 8) return $"{(Year - 1) % 100:00}{Year % 100:00}";
        else return $"{Year % 100:00}{(Year + 1) % 100:00}";
    }

    public static string GetAcademicYear(string stringDate)
    {
        var date = stringDate == TokenisableYearConstants.CurrentDate ? DateTime.Now
            : stringDate == TokenisableYearConstants.CurrentAyToken ? DateTime.Now
            : stringDate == TokenisableYearConstants.NextAyToken ? DateTime.Now.AddYears(1)
            : stringDate == TokenisableYearConstants.PreviousAyToken ? DateTime.Now.AddYears(-1)
            : stringDate == TokenisableYearConstants.CurrentAyPlusTwoToken ? DateTime.Now.AddYears(2)
            : default;

        return CalculateAcademicYear("0", date);
    }

    public static Dictionary<string, byte> Months = new()
    {
        { "January", 1 },
        { "February", 2 },
        { "March", 3 },
        { "April", 4 },
        { "May", 5 },
        { "June", 6 },
        { "July", 7 },
        { "August", 8 },
        { "September", 9 },
        { "October", 10 },
        { "November", 11 },
        { "December", 12 }
    };

    public static Dictionary<string, byte> Period = new()
    {
        { "January", 6 },
        { "February", 7 },
        { "March", 8 },
        { "April", 9 },
        { "May", 10 },
        { "June", 11 },
        { "July", 12 },
        { "August", 1 },
        { "September", 2 },
        { "October", 3 },
        { "November", 4 },
        { "December", 5 }
    };
}