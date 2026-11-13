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
        var parts = value.Split('-');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid period string format for TokenisablePeriod.");
        }

        var yearPortion = parts[0];
        var periodPortion = parts[1];
        if (periodPortion.StartsWith('R'))
            periodPortion = periodPortion[1..];

        if(!byte.TryParse(periodPortion, out var period) || period > 14)
            throw new ArgumentException("Invalid period string format for TokenisablePeriod.");


        if (yearPortion.ToLower() == TokenisableYearConstants.CurrentAyToken.ToLower())
        {
            return new TokenisablePeriod(new Period(TokenisableAcademicYear.GetAcademicYear(0), period));
        }

        if (yearPortion.ToLower() == TokenisableYearConstants.PreviousAyToken.ToLower())
        {
            return new TokenisablePeriod(new Period(TokenisableAcademicYear.GetAcademicYear(-1), period));
        }

        if (yearPortion.ToLower() == TokenisableYearConstants.NextAyToken.ToLower())
        {
            return new TokenisablePeriod(new Period(TokenisableAcademicYear.GetAcademicYear(1), period));
        }

        if (yearPortion.ToLower() == TokenisableYearConstants.CurrentAyPlusTwoToken.ToLower())
        {
            return new TokenisablePeriod(new Period(TokenisableAcademicYear.GetAcademicYear(2), period));
        }

        if (yearPortion.ToLower() == TokenisableYearConstants.TwoYearsAgoAYToken.ToLower())
        {
            return new TokenisablePeriod(new Period(TokenisableAcademicYear.GetAcademicYear(-2), period));
        }

        if (short.TryParse(yearPortion, out var academicYear))
        {
            return new TokenisablePeriod(new Period(academicYear, period));
        }

        throw new ArgumentException("Invalid string format for TokenisablePeriod.");
    }
}