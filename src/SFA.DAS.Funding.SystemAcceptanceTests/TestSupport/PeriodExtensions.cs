namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public static class PeriodExtensions
{
	public static Period GetPreviousPeriod(this Period period)
	{
		if (period.PeriodValue != 1)
		{
			return new Period(period.AcademicYear, (byte)(period.PeriodValue - 1));
		}
		else
		{
			return new Period(period.AcademicYear.GetPreviousAcademicYear(), 12);
		}
	}

	public static Period GetNextPeriod(this Period period)
	{
		if (period.PeriodValue != 12)
		{
			return new Period(period.AcademicYear, (byte)(period.PeriodValue + 1));
		}
		else
		{
			return new Period(period.AcademicYear.GetNextAcademicYear(), 1);
		}
	}

    public static string ToCollectionPeriodString(this Period period)
    {
        return $"{period.AcademicYear}-R{period.PeriodValue:D2}";
    }

	public static bool IsBefore(this Period period, Period comparisonPeriod)
    {
        if (period.AcademicYear < comparisonPeriod.AcademicYear)
        {
            return true;
        }
        else if (period.AcademicYear == comparisonPeriod.AcademicYear)
        {
            return period.PeriodValue < comparisonPeriod.PeriodValue;
        }
        return false;
    }

	public static bool IsAfter(this Period period, Period comparisonPeriod)
    {
        if (period.AcademicYear > comparisonPeriod.AcademicYear)
        {
            return true;
        }
        else if (period.AcademicYear == comparisonPeriod.AcademicYear)
        {
            return period.PeriodValue > comparisonPeriod.PeriodValue;
        }
        return false;
    }
}