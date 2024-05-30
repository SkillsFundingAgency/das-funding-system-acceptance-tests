namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

/// <summary>
/// This can be used to represent either a Collection or Delivery period.
/// </summary>
public class Period
{
	/// <summary>
	/// Initialises Period from a given academic year and period value
	/// </summary>
	/// <param name="academicYear">Academic year of the period</param>
	/// <param name="periodValue">Period value (e.g. 1 for R01/August)</param>
	public Period(short academicYear, byte periodValue)
	{
		AcademicYear = academicYear;
		PeriodValue = periodValue;
	}

	/// <summary>
	/// Initialises Period from a given date
	/// </summary>
	/// <param name="date">Date during period (e.g. 4/9/2024 for 2425-R02)</param>
	public Period(DateTime date)
	{
		var shortYear = int.Parse(date.Year.ToString().Substring(2));

		if (date.Month < 7)
		{
			AcademicYear = short.Parse($"{shortYear - 1}{shortYear}");
			PeriodValue = (byte)(date.Month + 5);
		}
		else
		{
			AcademicYear = short.Parse($"{shortYear}{shortYear + 1}");
			PeriodValue = (byte)(date.Month - 7);
		}
	}

	public short AcademicYear { get; set; }
	public byte PeriodValue { get; set; }
}