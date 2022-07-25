using FluentAssertions.Collections;
using FluentAssertions.Execution;

namespace SFA.DAS.Funding.IntegrationTests.Helpers;

public static class FundingPeriodAssertionsExtensions
{
    public static FundingPeriodAssertions Should(this IEnumerable<FundingPeriod> instance)
    {
        return new FundingPeriodAssertions(instance);
    }
}

public class FundingPeriodAssertions : GenericCollectionAssertions<FundingPeriod>
{
    public AndConstraint<GenericCollectionAssertions<FundingPeriod>> HaveAdjustedAgreedPriceOf(decimal targetValue,
        string because = "", params object[] becauseArgs)
    {
        var subjArray = Subject.ToArray();
        for (var i = 0; i < subjArray.Length; i++)
        {
            Execute.Assertion
                .Given(() => subjArray[i])
                .ForCondition(v => v.AgreedPrice == targetValue)
                .FailWith(
                    $"Expected value {subjArray[i].AgreedPrice}] in Period[{i}] should be {targetValue}");
        }

        return new AndConstraint<GenericCollectionAssertions<FundingPeriod>>(this);
    }

    protected override string Identifier => "agreed price";

    public FundingPeriodAssertions(IEnumerable<FundingPeriod> actualValue) : base(actualValue)
    {
    }
}