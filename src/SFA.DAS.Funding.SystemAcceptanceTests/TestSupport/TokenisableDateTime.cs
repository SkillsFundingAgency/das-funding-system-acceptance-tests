﻿using System.ComponentModel;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

[TypeConverter(typeof(TokenisableDateTimeConverter))]
public class TokenisableDateTime
{
    

    public TokenisableDateTime(DateTime value)
	{
		Value = value;
	}

	public DateTime Value { get; }

	public static TokenisableDateTime FromString(string value)
	{
		if (DateTime.TryParse(value, out var parseResult))
		{
			return new TokenisableDateTime(parseResult);
		}

        if (value.ToLower() == TokenisableYearConstants.CurrentDate.ToLower())
        {
            return new TokenisableDateTime(DateTime.Now);
        }

        if (value.ToLower() == TokenisableYearConstants.NextMonthFirstDay.ToLower())
        {
            var nextMonth = DateTime.Now.AddMonths(1);
            var firstDayOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);
            return new TokenisableDateTime(firstDayOfNextMonth);
        }

        if (value.ToLower() == TokenisableYearConstants.LastDayOfCurrentMonth.ToLower())
        {
            var firstDayOfNextMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            var lastDayOfCurrentMonth = firstDayOfNextMonth.AddDays(-1);
            return new TokenisableDateTime(lastDayOfCurrentMonth);
        }

        var dateComponents = value.Split('-');

        if (dateComponents[0].ToLower() == TokenisableYearConstants.CurrentAyToken.ToLower())
		{
			return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents));
		}

		if (dateComponents[0].ToLower() == TokenisableYearConstants.PreviousAyToken.ToLower())
		{
			return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(-1));
		}

		if (dateComponents[0].ToLower() == TokenisableYearConstants.NextAyToken.ToLower())
		{
			return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(1));
		}

        if (dateComponents[0].ToLower() == TokenisableYearConstants.CurrentAyPlusTwoToken.ToLower())
        {
            return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(2));
        }

        if (dateComponents[0].ToLower() == TokenisableYearConstants.TwoYearsAgoAYToken.ToLower())
        {
            return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(-2));
        }

        throw new ArgumentException("Invalid date string format for TokenisableDateTime.");
	}

	private static DateTime GetDateForCurrentAcademicYear(string[] dateComponents)
	{
		if (!int.TryParse(dateComponents[1], out var month)) throw new ArgumentException("Invalid date string format for TokenisableDateTime: Invalid month.");
		if (!int.TryParse(dateComponents[2], out var day)) throw new ArgumentException("Invalid date string format for TokenisableDateTime: Invalid day.");

		int startYearOfCurrentAcademicYear = DateTime.Now.Month > 7 ? DateTime.Now.Year : DateTime.Now.Year - 1;
		int yearToUse = month > 7 ? startYearOfCurrentAcademicYear : startYearOfCurrentAcademicYear + 1;
		return new DateTime(yearToUse, month, day);
	}
}