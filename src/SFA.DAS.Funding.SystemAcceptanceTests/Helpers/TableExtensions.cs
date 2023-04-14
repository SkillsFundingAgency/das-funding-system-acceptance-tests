namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class TableExtensions
{
    public static List<(byte Period, short AcademicYear, byte Month)> ToExpectedPeriods(this Table table)
    {
        return table.Rows.Select(row => 
                (Convert.ToByte(row[0]), Convert.ToInt16(row[1]), Months[row[2]]))
            .ToList();
    }

    public static List<(short AcademicPeriod, byte DeliveryPeriod, decimal Amount, short PaymentYear, byte PaymentPeriod)> ToExpectedPayments(this Table table)
    {
        return table.Rows.Select(row =>
                (Convert.ToInt16(row[0]), Period[row[1]], Convert.ToDecimal(row[2]), Convert.ToInt16(row[3]), Period[row[4]]))
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