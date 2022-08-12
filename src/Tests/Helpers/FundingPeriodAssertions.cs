using FluentAssertions.Collections;
using FluentAssertions.Execution;

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
                .ForCondition(v => v.AgreedPrice*0.80m == targetValue)
                .FailWith(
                    $"Expected value {subjArray[i].AgreedPrice} in Period[{i+1}] should be {targetValue}");
        }

        return new AndConstraint<GenericCollectionAssertions<FundingPeriod>>(this);
    }

    protected override string Identifier => "agreed price";

    public FundingPeriodAssertions(IEnumerable<FundingPeriod> actualValue) : base(actualValue)
    {
    }
}