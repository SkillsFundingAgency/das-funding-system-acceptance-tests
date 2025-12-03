using System.ComponentModel;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

[TypeConverter(typeof(TokenisableAcademicYearConverter))]
public class TokenisableAcademicYear
{
    public TokenisableAcademicYear(short value)
    {
        Value = value;
    }

    public short Value;

    public static TokenisableAcademicYear FromString(string value)
    {
        if (value == "null") return new TokenisableAcademicYear(0);

        if (value.ToLower() == TokenisableYearConstants.CurrentAyToken.ToLower())
        {
            return new TokenisableAcademicYear(GetAcademicYear(0));
        }

        if (value.ToLower() == TokenisableYearConstants.PreviousAyToken.ToLower())
        {
            return new TokenisableAcademicYear(GetAcademicYear(-1));
        }

        if (value.ToLower() == TokenisableYearConstants.NextAyToken.ToLower())
        {
            return new TokenisableAcademicYear(GetAcademicYear(1));
        }

        if (value.ToLower() == TokenisableYearConstants.CurrentAyPlusTwoToken.ToLower())
        {
            return new TokenisableAcademicYear(GetAcademicYear(2));
        }

        if (value.ToLower() == TokenisableYearConstants.TwoYearsAgoAYToken.ToLower())
        {
            return new TokenisableAcademicYear(GetAcademicYear(-2));
        }

        return short.TryParse(value, out var parsedAy)
            ? new TokenisableAcademicYear(parsedAy)
            : throw new ArgumentException("Invalid string format for TokenisableAcademicYear.");
    }

    public static short GetAcademicYear(int offsetByYears)
    {
        int startYearOfAcademicYear = DateTime.Now.Month > 7 ? DateTime.Now.Year + offsetByYears : DateTime.Now.Year - 1 + offsetByYears;
        var twoDigitStartYear = short.Parse(startYearOfAcademicYear.ToString().Substring(2, 2));
        return short.Parse($"{twoDigitStartYear}{twoDigitStartYear + 1}");
    }
}