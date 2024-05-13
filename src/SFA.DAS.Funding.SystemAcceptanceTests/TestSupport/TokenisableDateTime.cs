﻿using System.ComponentModel;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

[TypeConverter(typeof(TokenisableDateTimeConverter))]
public class TokenisableDateTime
{
	private const string CurrentAyToken = "currentAY";
	private const string PreviousAyToken = "previousAY";
	private const string NextAyToken = "nextAY";

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

		var dateComponents = value.Split('-');

		if (dateComponents[0].ToLower() == CurrentAyToken.ToLower())
		{
			return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents));
		}

		if (dateComponents[0].ToLower() == PreviousAyToken.ToLower())
		{
			return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(-1));
		}

		if (dateComponents[0].ToLower() == NextAyToken.ToLower())
		{
			return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(1));
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