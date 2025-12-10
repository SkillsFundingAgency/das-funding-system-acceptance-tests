using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class BreakInLearningStepDefinitions(ScenarioContext context)
{
    [Given("SLD inform us of a break in learning with pause date (.*)")]
    [When("SLD inform us of a break in learning with pause date (.*)")]
    public void SLDInformUsOfABreakInLearningWithPauseDate(TokenisableDateTime pauseDate)
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithPauseDate(pauseDate.Value);
    }

    [Given("SLD inform us of a return from break in learning with a new learning start date (.*)")]
    [When("SLD inform us of a return from break in learning with a new learning start date (.*)")]
    public void SLDInformUsOfAReturnFromBreakInLearningWithANewLearningStartDate(TokenisableDateTime newLearningStartDate)
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithReturnFromBreakInLearning(newLearningStartDate.Value);
    }

    [When("SLD inform us of a correction to a previously recorded return from break in learning with a new learning start date (.*)")]
    public void SLDInformUsOfACorrectionToReturnFromBreakInLearningWithANewLearningStartDate(TokenisableDateTime newLearningStartDate)
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithReturnFromBreakInLearning(newLearningStartDate.Value, correction:true);
    }

    [When("SLD inform us that a previously recorded return from a break in learning is removed")]
    public void SLDInformUsAPreviouslyRecordedBreakInLearningReturnIsRemoved()
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithBreakInLearningReturnRemoved();
    }

    [When("SLD inform us that an entire previously recorded break in learning and return is removed")]
    public void SLDInformUsAPreviouslyRecordedBreakInLearningAndReturnIsRemoved()
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithEntireBreakInLearningRemoved();
    }

    [Then("the earnings between (.*) and (.*) are soft deleted")]
    public void EarningsBetweenDeliveryPeriodsAreSoftDeleted(TokenisablePeriod firstPeriod, TokenisablePeriod secondPeriod)
    {
        var testData = context.Get<TestData>();

        testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?
            .AssertBetweenRange(
                firstPeriod.Value,
                secondPeriod.Value, 
                x => x.IsAfterLearningEnded, 
                x => $"Found instalment for AcademicYear {x.AcademicYear} DeliveryPeriod {x.DeliveryPeriod} that is not soft deleted in earnings db.");
    }

    [Then("the earnings between (.*) and (.*) are maintained")]
    public void EarningsBetweenDeliveryPeriodsAreMaintained(TokenisablePeriod firstPeriod, TokenisablePeriod secondPeriod)
    {
        var testData = context.Get<TestData>();

        testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?
            .AssertBetweenRange(
                firstPeriod.Value,
                secondPeriod.Value,
                x => !x.IsAfterLearningEnded,
                x => $"Found instalment for AcademicYear {x.AcademicYear} DeliveryPeriod {x.DeliveryPeriod} that has been soft deleted in earnings db.");
    }

    [Then("the earnings of (.*) between (.*) and (.*) are maintained")]
    public void EarningsOfAmountBetweenDeliveryPeriodsAreMaintained(decimal amount, TokenisablePeriod firstPeriod, TokenisablePeriod secondPeriod)
    {
        var testData = context.Get<TestData>();

        var instalments = testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments
            ?.OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod).ToList();

        testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?
            .AssertBetweenRange(
                firstPeriod.Value,
                secondPeriod.Value,
                x => Math.Round(x.Amount, 2) == Math.Round(amount, 2) && !x.IsAfterLearningEnded,
                x => $"Expected instalment of amount {amount} for AcademicYear {x.AcademicYear} DeliveryPeriod {x.DeliveryPeriod} but one was not found, the wrong amount, or soft deleted.");
    }
}