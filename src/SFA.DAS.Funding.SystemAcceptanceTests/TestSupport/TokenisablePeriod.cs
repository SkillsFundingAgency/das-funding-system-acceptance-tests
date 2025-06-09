using System.ComponentModel;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

[TypeConverter(typeof(TokenisablePeriodConverter))]
public class TokenisablePeriod
{
    public TokenisablePeriod(Period value)
    {
        Value = value;
    }

    public Period Value;

    public static TokenisablePeriod FromString(string value)
    {
        var yearPortion = value.Split('-')[0];
        var periodPortion = value.Split('-')[1];
        if (periodPortion.StartsWith('R'))
            periodPortion = periodPortion[1..];

        if(!byte.TryParse(periodPortion, out var period) || period > 14)
            throw new ArgumentException("Invalid period string format for TokenisablePeriod.");


        if (yearPortion.ToLower() == TokenisableYearConstants.CurrentAyToken.ToLower())
        {
            return new TokenisablePeriod(new Period(GetAcademicYear(0), period));
        }

        if (yearPortion.ToLower() == TokenisableYearConstants.PreviousAyToken.ToLower())
        {
            return new TokenisablePeriod(new Period(GetAcademicYear(-1), period));
        }

        if (yearPortion.ToLower() == TokenisableYearConstants.NextAyToken.ToLower())
        {
            return new TokenisablePeriod(new Period(GetAcademicYear(1), period));
        }

        if (yearPortion.ToLower() == TokenisableYearConstants.CurrentAyPlusTwoToken.ToLower())
        {
            return new TokenisablePeriod(new Period(GetAcademicYear(2), period));
        }

        if (yearPortion.ToLower() == TokenisableYearConstants.TwoYearsAgoAYToken.ToLower())
        {
            return new TokenisablePeriod(new Period(GetAcademicYear(-2), period));
        }

        throw new ArgumentException("Invalid string format for TokenisablePeriod.");
    }

    public static short GetAcademicYear(int offsetByYears)
    {
        int startYearOfAcademicYear = DateTime.Now.Month > 7 ? DateTime.Now.Year + offsetByYears : DateTime.Now.Year - 1 + offsetByYears;
        var twoDigitStartYear = short.Parse(startYearOfAcademicYear.ToString().Substring(2, 2));
        return short.Parse($"{twoDigitStartYear}{twoDigitStartYear + 1}");
    }
}