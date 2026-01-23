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
    [Given("SLD inform us of a return from break in learning with both a new learning start date (.*) and new expected end date (.*)")]
    [When("SLD inform us of a return from break in learning with both a new learning start date (.*) and new expected end date (.*)")]
    public void SLDInformUsOfAReturnFromBreakInLearningWithANewLearningStartDateAndExpectedEndDate(TokenisableDateTime newLearningStartDate, TokenisableDateTime newExpectedEndDate)
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithReturnFromBreakInLearning(newLearningStartDate.Value, newExpectedEndDate:newExpectedEndDate.Value);
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

    [Then("the earnings of (.*) between (.*) and (.*) are maintained")]
    public void EarningsOfAmountBetweenDeliveryPeriodsAreMaintained(decimal amount, TokenisablePeriod firstPeriod, TokenisablePeriod secondPeriod)
    {
        var testData = context.Get<TestData>();

        var instalments = testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments
            ?.OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod).ToList();

        instalments?
            .AssertBetweenRange(
                firstPeriod.Value,
                secondPeriod.Value,
                x => Math.Round(x.Amount, 2) == Math.Round(amount, 2),
                x => $"Expected instalment of amount {amount} for AcademicYear {x.AcademicYear} DeliveryPeriod {x.DeliveryPeriod} but one was not found, the wrong amount, or soft deleted.");
    }

    [Then("earnings are updated with (first|second) period in learning from (.*) to (.*)")]
    public void EarningsAreUpdatedWithPeriodInLearning(string periodNumber, TokenisableDateTime startDate, TokenisableDateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(periodNumber))
        {
            throw new ArgumentException("periodNumber cannot be null or empty.", nameof(periodNumber));
        }

        var normalisedPeriod = periodNumber.Trim().ToLowerInvariant();

        if (normalisedPeriod != "first" && normalisedPeriod != "second")
        {
            throw new ArgumentException(
                $"Invalid periodNumber '{periodNumber}'. Expected 'first' or 'second' (case-insensitive).",
                nameof(periodNumber));
        }

        var testData = context.Get<TestData>();

        var periodsInLearning = testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault().EpisodePeriodInLearning
            ?.OrderBy(x => x.StartDate).ToList();

        var index = normalisedPeriod == "first" ? 0 : 1;

        var period = periodsInLearning[index];

        Assert.AreEqual(startDate.Value.Date, period.StartDate.Date, $"{normalisedPeriod} Period in learning start date mismatch!");

        Assert.AreEqual(endDate.Value.Date, period.EndDate.Date, $"{normalisedPeriod} Period in learning end date mismatch!");
    }


    [Then("Break in Learning record is removed from earnings db")]
    public void BreakInLearningRecordIsRemovedFromEarningsDb()
    {
        var testData = context.Get<TestData>();

        var breakInLearnings = testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EpisodePeriodInLearning;

        Assert.IsTrue(breakInLearnings?.Count == 0, "Unexpected Break in Learnings records found for the apprenticeship");
    }
}