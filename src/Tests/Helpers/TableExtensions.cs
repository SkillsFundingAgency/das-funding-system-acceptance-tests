namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class TableExtensions
{
    public static List<(byte Period, short AcademicYear, byte Month)> ToExpectedPeriods(this Table table)
    {
        return table.Rows.Select(row => 
                (Convert.ToByte(row[0]), Convert.ToInt16(row[1]), Months[row[2]]))
            .ToList();
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
}