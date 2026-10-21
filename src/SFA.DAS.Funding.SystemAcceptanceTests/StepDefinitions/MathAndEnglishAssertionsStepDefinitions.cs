using Microsoft.IdentityModel.Tokens;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class MathAndEnglishAssertionsStepDefinitions(
    ScenarioContext context,
    EarningsSqlClient earningsEntitySqlClient)
{
    [Given("Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*)")]
    [When("Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*)")]
    public async Task AddMathsAndEnglishLearning(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithEnglishAndMaths(startDate.Value, endDate.Value, learnAimRef);

        testData.IsMathsAndEnglishAdded = true;
    }

    [When("English and Maths learning is recorded from (.*) to (.*) with learnAimRef (.*) and completion on (.*)")]
    public async Task AddMathsAndEnglishLearningWithCompletion(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, TokenisableDateTime completionDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithEnglishAndMaths(startDate.Value, endDate.Value, learnAimRef, completionDate: completionDate.Value);

        testData.IsMathsAndEnglishAdded = true;
    }

    [Given("a Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*) and learning support from (.*) to (.*)")]
    [When("a Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*) and learning support from (.*) to (.*)")]
    public async Task AddMathsAndEnglishLearningSupport(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef,
        TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithMathsAndEnglish(me => me.WithCourseDetails(startDate.Value, endDate.Value, learnAimRef, 2)
                    .WithLearningSupport(learningSupportStartDate.Value, learningSupportEndDate.Value));

        testData.IsMathsAndEnglishAdded = true;
    }


    [When("English and Maths learning is recorded from (.*) to (.*) with learnAimRef (.*), withdrawal date (.*), learning support from (.*) to (.*)")]
    public async Task AddMathsAndEnglishWithWithdrawalAndLearningSupport(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, TokenisableDateTime withdrawalDate,
        TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithMathsAndEnglish(me => me.WithCourseDetails(startDate.Value, endDate.Value, learnAimRef, 2)
                    .WithWithdrawalDate(withdrawalDate.Value)
                    .WithLearningSupport(learningSupportStartDate.Value, learningSupportEndDate.Value));

        testData.IsMathsAndEnglishAdded = true;
    }

    [Given("English and Maths learning is recorded from (.*) to (.*) with learnAimRef (.*), pause date (.*), learning support from (.*) to (.*)")]
    [When("English and Maths learning is recorded from (.*) to (.*) with learnAimRef (.*), pause date (.*), learning support from (.*) to (.*)")]
    public async Task AddMathsAndEnglishWithPauseAndLearningSupport(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, TokenisableDateTime pauseDate,
        TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithMathsAndEnglish(me => me.WithCourseDetails(startDate.Value, endDate.Value, learnAimRef, 2)
                    .WithPauseDate(pauseDate.Value)
                    .WithLearningSupport(learningSupportStartDate.Value, learningSupportEndDate.Value));

        testData.IsMathsAndEnglishAdded = true;
    }

    [Given("SLD record a return from break in learning for English and Maths course with new start date (.*)")]
    [When("SLD record a return from break in learning for English and Maths course with new start date (.*)")]
    public async Task WithEnglishAndMathsReturnFromBreakInLearning(TokenisableDateTime newStartDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithEnglishAndMathsReturnFromBreakInLearning(newStartDate.Value);

        testData.IsMathsAndEnglishAdded = true;
    }

    [When("SLD record a return from break in learning for English and Maths course with a new start date (.*) and end date (.*)")]
    public async Task WithEnglishAndMathsReturnFromBreakInLearningWithExpectedEndDate(TokenisableDateTime newStartDate, TokenisableDateTime expectedEndDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithEnglishAndMathsReturnFromBreakInLearning(newStartDate.Value, false, expectedEndDate.Value);

        testData.IsMathsAndEnglishAdded = true;
    }

    [When("SLD record a return from break in learning for English and Maths course with a new start date (.*) and withdrawal date (.*)")]
    public void WithEnglishAndMathsReturnFromBreakInLearningWithWithdrawalDate(TokenisableDateTime newStartDate, TokenisableDateTime withdrawalDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithEnglishAndMathsReturnFromBreakInLearning(newStartDate.Value, false, null, null, withdrawalDate.Value);

        testData.IsMathsAndEnglishAdded = true;
    }

    [When("SLD record a return from break in learning for English and Maths course with a new start date (.*) and completion date (.*)")]
    public void WithEnglishAndMathsReturnFromBreakInLearningWithCompletionDate(TokenisableDateTime newStartDate, TokenisableDateTime completionDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithEnglishAndMathsReturnFromBreakInLearning(newStartDate.Value, false, null, null, null, completionDate.Value);

        testData.IsMathsAndEnglishAdded = true;
    }



    [When("SLD inform us of a correction to an English and Maths return from break in learning with new start date (.*)")]
    public async Task CorrectionToEnglishAndMathsReturnFromBreakInLearning(TokenisableDateTime newStartDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithEnglishAndMathsReturnFromBreakInLearning(newStartDate.Value, true);

        testData.IsMathsAndEnglishAdded = true;
    }

    [When("SLD inform us that a previously recorded english and maths return from break in learning is removed")]
    public void PreviouslyRecordedEnglishAndMathsReturnFromBreakInLearningIsRemoved()
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithEnglishAndMathsReturnFromBreakInLearningRemoved();
    }


    [When("an English and Maths learning is recorded from (.*) to (.*) with learnAimRef (.*), completion date as (.*), learning support from (.*) to (.*)")]
    public async Task AddMathsAndEnglishWithCompletionAndLearningSupport(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, TokenisableDateTime completionDate,
        TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithMathsAndEnglish(me => me.WithCourseDetails(startDate.Value, endDate.Value, learnAimRef, 2)
        .WithCompletionDate(completionDate.Value)
        .WithLearningSupport(learningSupportStartDate.Value, learningSupportEndDate.Value));

        testData.IsMathsAndEnglishAdded = true;
    }

    [Given("Maths and English learning is recorded from (.*) for (.*) days with learnAimRef (.*) and withdrawal after (.*) days")]
    [When("Maths and English learning is recorded from (.*) for (.*) days with learnAimRef (.*) and withdrawal after (.*) days")]
    public async Task AddMathsAndEnglishLearningWithWithdrawal(TokenisableDateTime startDate, int duration, string learnAimRef, int withdrawalOnDay)
    {
        var testData = context.Get<TestData>();

        var endDate = startDate.Value.AddDays(duration - 1);
        var withdrawalDate = startDate.Value.AddDays(withdrawalOnDay - 1);

        var learnerBuilder = testData.GetLearnerDataBuilder();

        learnerBuilder.WithEnglishAndMaths(startDate.Value, endDate, learnAimRef, withdrawalDate: withdrawalDate);

        testData.IsMathsAndEnglishAdded = true;
    }


    [When("Math and English learning is recorded from (.*) to (.*) with learnAimRef (.*) and prior learning adjustment of (.*) percent and other funding adjustment of (.*) percent")]
    public async Task AddMathsAndEnglishLearning(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, string? priorLearning, string? otherFunding)
    {
        var testData = context.Get<TestData>();
        var learnerBuilder = testData.GetLearnerDataBuilder();

        int? priorLearningPercentage = ToNullableInt(priorLearning);
        int? otherFundingPercentage = ToNullableInt(otherFunding);


        learnerBuilder.WithEnglishAndMaths(startDate.Value, endDate.Value, learnAimRef, 2, null, null,
            priorLearningPercentage, otherFundingPercentage);

        testData.IsMathsAndEnglishAdded = true;
    }

    [Given("Maths and English earnings are generated from periods (.*) to (.*) with instalment amount (.*) for learnAimRef (.*)")]
    [When("Maths and English earnings are generated from periods (.*) to (.*) with instalment amount (.*) for learnAimRef (.*)")]
    [Then("Maths and English earnings are generated from periods (.*) to (.*) with instalment amount (.*) for learnAimRef (.*)")]
    public async Task VerifyMathsAndEnglishInstalmentEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
        TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount, string learnAimRef)
    {
        await VerifyMathsAndEnglishEarnings(mathsAndEnglishStartPeriod, mathsAndEnglishEndPeriod, amount, learnAimRef,
            true);
    }

    [When("SLD inform us that Maths and English details have changed")]
    public void SLDInformUsThatMathsAndEnglishDetailsHaveChanged()
    {
        var testData = context.Get<TestData>();

        testData.ResetLearnerDataBuilder();
    }

    [Given("Maths and English earnings are generated from periods (.*) to (.*) with regular instalment amount (.*) for learnAimRef (.*)")]
    [When("Maths and English earnings are generated from periods (.*) to (.*) with regular instalment amount (.*) for learnAimRef (.*)")]
    [Then("Maths and English earnings are generated from periods (.*) to (.*) with regular instalment amount (.*) for learnAimRef (.*)")]
    public async Task VerifyRegularMathsAndEnglishInstalmentEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
        TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount, string learnAimRef)
    {
        await VerifyMathsAndEnglishEarnings(mathsAndEnglishStartPeriod, mathsAndEnglishEndPeriod, amount, learnAimRef,
            false);
    }

    [When(@"the maths and english courses are removed")]
    public void WhenTheMathsAndEnglishCoursesAreRemoved()
    {
        var testData = context.Get<TestData>();
        var learnerBuilder = testData.GetLearnerDataBuilder();
        learnerBuilder.WithNoMathsAndEnglish();
    }

    [Then(@"no maths and english earnings are generated")]
    public async Task ThenNoMathsAndEnglishEarningsAreGenerated()
    {
        var testData = context.Get<TestData>();
        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = earningsEntitySqlClient.GetApprenticeshipEarningsEntityModel(context);
            return !testData.IsMathsAndEnglishAdded || earningsApprenticeshipModel.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)
                .EarningsProfileHistory.Any();
        }, "Failed to find updated earnings entity.");

        var mathsAndEnglish = earningsApprenticeshipModel
            .Episodes
            .GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)
            ?.MathsAndEnglish;

        var mathAndEnglishInstalments = earningsApprenticeshipModel
            .Episodes
            .GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)
            ?.MathsAndEnglishInstalments;

        mathsAndEnglish?.Where(x => !x.IsRemoved).Should().BeEmpty("Expected no Maths and English earnings to be generated, but found some on the earnings apprenticeship model");
        mathAndEnglishInstalments.Should().BeEmpty("Expected no Maths and English instalments to be generated, but found some on the earnings apprenticeship model");
    }

    private async Task VerifyMathsAndEnglishEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
        TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount, string learnAimRef, bool assertNoSubsequentEarningsExist, bool assertNoPriorEarningsExist = false)
    {
        var testData = context.Get<TestData>();
        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = earningsEntitySqlClient.GetApprenticeshipEarningsEntityModel(context);
            return !testData.IsMathsAndEnglishAdded || earningsApprenticeshipModel.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)
                .EarningsProfileHistory.Any();
        }, "Failed to find updated earnings entity.");


        var epidsode = earningsApprenticeshipModel.Episodes.GetEpisode(testData);

        var mathsAndEnglish = epidsode?.MathsAndEnglish;

        var mathsAndEnglishKey = mathsAndEnglish.FirstOrDefault(x => x.LearnAimRef.Contains(learnAimRef)).Key;


        var mathsAndEnglishInstalments = epidsode
            ?.MathsAndEnglishInstalments.Where(x => x.MathsAndEnglishKey == mathsAndEnglishKey)
            .Where(x => x.Type == "Regular")
            .ToList();

        testData.EarningsProfileId = epidsode.EarningsProfile.EarningsProfileId;

        mathsAndEnglishInstalments.Should()
            .NotBeNull("No Maths and English instalment data found on earnings apprenticeship model");

        if (assertNoPriorEarningsExist)
        {
            mathsAndEnglishInstalments.Should().NotContain(x =>
            new Period(x.AcademicYear, x.DeliveryPeriod).IsBefore(mathsAndEnglishStartPeriod.Value),
            $"Expected no Maths and English earnings before {mathsAndEnglishStartPeriod.Value.ToCollectionPeriodString()}");

        }

        if (assertNoSubsequentEarningsExist)
        {
            mathsAndEnglishInstalments.Should().NotContain(x =>
                    mathsAndEnglishEndPeriod.Value.IsBefore(new Period(x.AcademicYear, x.DeliveryPeriod)),
                $"Expected no Maths and English earnings after {mathsAndEnglishEndPeriod.Value.ToCollectionPeriodString()}");
        }

        while (mathsAndEnglishStartPeriod.Value.IsBefore(mathsAndEnglishEndPeriod.Value.GetNextPeriod()))
        {
            mathsAndEnglishInstalments.Should().Contain(x =>
                    x.MathsAndEnglishKey == mathsAndEnglishKey
                    && x.Amount == amount
                    && x.AcademicYear == mathsAndEnglishStartPeriod.Value.AcademicYear
                    && x.DeliveryPeriod == mathsAndEnglishStartPeriod.Value.PeriodValue,
                $"Expected Maths and English earning for {mathsAndEnglishStartPeriod.Value.ToCollectionPeriodString()}");

            mathsAndEnglishStartPeriod.Value = mathsAndEnglishStartPeriod.Value.GetNextPeriod();
        }
    }

    [When("a Maths and English balancing earning of (.*) is generated for learnAimRef (.*) for period (.*)")]
    [Then("a Maths and English balancing earning of (.*) is generated for learnAimRef (.*) for period (.*)")]
    public async Task VerifyMathsAndEnglishEarnings(decimal amount, string learnAimRef, TokenisablePeriod period)
    {
        var mathsAndEnglishInstalments = GetLatestMathsAndEnglishInstalmentsOfType(learnAimRef, "Balancing");

        mathsAndEnglishInstalments.Should()
            .NotBeNull("No Maths and English balancing instalment data found on earnings apprenticeship model");

        mathsAndEnglishInstalments.Result.Should()
            .Contain(x =>
                x.DeliveryPeriod == period.Value.PeriodValue &&
                x.AcademicYear == period.Value.AcademicYear &&
                x.Amount == amount);
    }

    [Then("No Maths and English balancing earning for learnAimRef (.*) is generated")]
    [Then("Maths and English balancing earning is removed for learnAimRef (.*)")]
    public void MathsAndEnglishBalancingEarningIsRemovedForCourse(string learnAimRef)
    {

        var mathsAndEnglishInstalments = GetLatestMathsAndEnglishInstalmentsOfType(learnAimRef, "Balancing");

        Assert.Zero(mathsAndEnglishInstalments.Result.Count, "Unexpected Balancing earning for English and Maths found! ");
    }

    [Then("Maths and English earnings for learnAimRef (.*) are removed")]
    public async Task VerifyMathsAndEnglishEarningsAreRemoved(string learnAimRef)
    {
        var mathsAndEnglishInstalments = GetLatestMathsAndEnglishInstalmentsOfType(learnAimRef, "Regular");

        mathsAndEnglishInstalments.Result.Should().BeEmpty("Unexpected Maths and English instalment data found on earnings apprenticeship model");
    }

    private async Task<List<MathsAndEnglishInstalment>> GetLatestMathsAndEnglishInstalmentsOfType(string learnAimRef, string type)
    {
        var testData = context.Get<TestData>();
        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = earningsEntitySqlClient.GetApprenticeshipEarningsEntityModel(context);
            return !testData.IsMathsAndEnglishAdded
                   || earningsApprenticeshipModel.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)
                       .EarningsProfileHistory.Any();
        }, "Failed to find updated earnings entity.");

        var episode = earningsApprenticeshipModel
            .Episodes
            .GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent);

        if (episode == null || episode.MathsAndEnglish == null)
            return new List<MathsAndEnglishInstalment>();

        var mathsAndEnglishKey = episode.MathsAndEnglish
            .FirstOrDefault(x => x.LearnAimRef.Contains(learnAimRef))?
            .Key;

        if (mathsAndEnglishKey == null)
            return new List<MathsAndEnglishInstalment>();

        testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent).EarningsProfile.EarningsProfileId;

        return episode.MathsAndEnglishInstalments
            .Where(x => x.MathsAndEnglishKey == mathsAndEnglishKey && x.Type == type)
            .ToList();
    }

    private int? ToNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalised = value.Trim().ToLower();

        if (normalised == "null" || normalised == "none" || normalised == "n/a")
            return null;

        if (int.TryParse(value, out var result))
            return result;

        throw new ArgumentException($"'{value}' is not a valid integer.");
    }
}
