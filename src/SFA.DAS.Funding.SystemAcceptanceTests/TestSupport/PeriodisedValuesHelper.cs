﻿using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public static class PeriodisedValuesHelper
{
    public static IEnumerable<(byte PeriodNumber, decimal Value)> GetValuesForAttribute(this List<PriceEpisodePeriodisedValues> valueList, string attributeName)
    {
        var values = valueList.SingleOrDefault(x => x.AttributeName == attributeName);
        if (values == null)
            Assert.Fail($"No PriceEpisodePeriodisedValues found for attribute {attributeName}");

        foreach (var propertyInfo in typeof(PriceEpisodePeriodisedValues).GetProperties().Where(p => p.Name.StartsWith("Period")))
        {
            var periodNumber = byte.Parse(propertyInfo.Name.Substring(6));
            yield return (periodNumber, propertyInfo.GetValue(values).As<decimal>());
        }
    }
}