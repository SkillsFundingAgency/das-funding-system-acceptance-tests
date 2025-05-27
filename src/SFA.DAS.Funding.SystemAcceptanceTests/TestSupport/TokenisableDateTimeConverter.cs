﻿using System.ComponentModel;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

#pragma warning disable CS8765, CS8603
public class TokenisableDateTimeConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if (value is string str)
		{
			return TokenisableDateTime.FromString(str);
		}
		return base.ConvertFrom(context, culture, value);
	}
}
#pragma warning restore CS8765, CS8603