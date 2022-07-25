using FluentAssertions.Execution;
using FluentAssertions.Numeric;
using FluentAssertions.Primitives;

namespace SFA.DAS.Funding.IntegrationTests.Helpers
{
    public class FundingPeriodAssertions2 : ReferenceTypeAssertions<FundingPeriod, FundingPeriodAssertions2>
    {
        public AndConstraint<FundingPeriodAssertions2> HaveAdjustedAgreedPriceOf(decimal targetValue,
            string because = "", params object[] becauseArgs)
        {
                Execute.Assertion
                    .Given(() => Subject)
                    .ForCondition(v => v.AgreedPrice == targetValue)
                    .FailWith(
                        $"Expected value {Subject.AgreedPrice}] in Period With start date [{Subject.StartDate}] should be {targetValue}");

            return new AndConstraint<FundingPeriodAssertions2>(this);
        }


        public FundingPeriodAssertions2(FundingPeriod subject) : base(subject)
        {

        }

        protected override string Identifier => "agreed price";
    }
}
