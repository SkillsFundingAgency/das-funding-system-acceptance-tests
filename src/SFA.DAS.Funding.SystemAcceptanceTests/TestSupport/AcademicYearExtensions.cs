namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public static class AcademicYearExtensions
{
	public static int GetStartingYearFromAcademicYear(this short academicYear)
	{
		return int.Parse($"20{academicYear.ToString().Substring(0,2)}");
	}

	public static short GetAcademicYearFromStartingYear(this int startingYear)
	{
		return short.Parse($"{startingYear.ToString().Substring(2,2)}{(startingYear + 1).ToString().Substring(2,2)}");
	}

	public static short GetPreviousAcademicYear(this short academicYear)
	{
		var firstTwo = short.Parse(academicYear.ToString().Substring(0, 2));
		return short.Parse($"{firstTwo - 1}{firstTwo}");
	}

	public static short GetNextAcademicYear(this short academicYear)
	{
		var lastTwo = short.Parse(academicYear.ToString().Substring(2, 2));
		return short.Parse($"{lastTwo}{lastTwo + 1}");
	}
}