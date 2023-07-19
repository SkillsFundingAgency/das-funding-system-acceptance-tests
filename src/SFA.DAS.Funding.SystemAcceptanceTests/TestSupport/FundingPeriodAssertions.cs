using FluentAssertions.Collections;
using FluentAssertions.Execution;

public static class FundingPeriodAssertionsExtensions
{
    public static FundingPeriodAssertions Should(this IEnumerable<DeliveryPeriod> instance)
    {
        return new FundingPeriodAssertions(instance);
    }
}

public class FundingPeriodAssertions : GenericCollectionAssertions<DeliveryPeriod>
{
    public AndConstraint<GenericCollectionAssertions<DeliveryPeriod>> HaveAdjustedAgreedPriceOf(decimal targetValue,
        string because = "", params object[] becauseArgs)
    {
        var subjArray = Subject.ToArray();
        for (var i = 0; i < subjArray.Length; i++)
        {
            Execute.Assertion
                .Given(() => subjArray[i])
                .ForCondition(v => v.LearningAmount == targetValue)
                .FailWith(
                    $"Expected value {subjArray[i].LearningAmount} in Period[{i+1}] should be {targetValue}");
        }

        return new AndConstraint<GenericCollectionAssertions<DeliveryPeriod>>(this);
    }

    protected override string Identifier => "agreed price";

    public FundingPeriodAssertions(IEnumerable<DeliveryPeriod> actualValue) : base(actualValue)
    {
    }
}