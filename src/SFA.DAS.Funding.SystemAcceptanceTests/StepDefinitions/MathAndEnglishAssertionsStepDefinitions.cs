using Microsoft.IdentityModel.Tokens;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class MathAndEnglishAssertionsStepDefinitions(
        ScenarioContext context,
        EarningsSqlClient earningsEntitySqlClient)
    {
        [Given("Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*) and amount (.*)")]
        [When("Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*) and amount (.*)")]
        public async Task AddMathsAndEnglishLearning(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, string course, decimal amount)
        {
            var testData = context.Get<TestData>();

            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder.WithEnglishAndMaths(startDate.Value, endDate.Value, course, amount, learnAimRef);

            testData.IsMathsAndEnglishAdded = true;
        }

        [When("Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*), amount (.*) and completion on (.*)")]
        public async Task AddMathsAndEnglishLearningWithCompletion(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, string course, decimal amount, TokenisableDateTime completionDate)
        {
            var testData = context.Get<TestData>();

            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithEnglishAndMaths(startDate.Value, endDate.Value, course, amount, learnAimRef, completionDate: completionDate.Value);

            testData.IsMathsAndEnglishAdded = true;
        }

        [Given("a Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*), amount (.*), learning support from (.*) to (.*)")]
        [When("a Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*), amount (.*), learning support from (.*) to (.*)")]
        public async Task AddMathsAndEnglishLearningSupport(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, string course, decimal amount,
            TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
        {
            var testData = context.Get<TestData>();

            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithMathsAndEnglish(me => me.WithCourseDetails(startDate.Value, endDate.Value, course, amount, learnAimRef)
                        .WithLearningSupport(learningSupportStartDate.Value, learningSupportEndDate.Value));

            testData.IsMathsAndEnglishAdded = true;
        }


        [When("English and Maths learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*), amount (.*), withdrawal date (.*), learning support from (.*) to (.*)")]
        public async Task AddMathsAndEnglishWithWithdrawalAndLearningSupport(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, string course, decimal amount, TokenisableDateTime withdrawalDate,
            TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
        {
            var testData = context.Get<TestData>();

            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithMathsAndEnglish(me => me.WithCourseDetails(startDate.Value, endDate.Value, course, amount, learnAimRef)
                        .WithWithdrawalDate(withdrawalDate.Value)
                        .WithLearningSupport(learningSupportStartDate.Value, learningSupportEndDate.Value));

            testData.IsMathsAndEnglishAdded = true;
        }

        [Given("English and Maths learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*), amount (.*), pause date (.*), learning support from (.*) to (.*)")]
        [When("English and Maths learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*), amount (.*), pause date (.*), learning support from (.*) to (.*)")]
        public async Task AddMathsAndEnglishWithPauseAndLearningSupport(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, string course, decimal amount, TokenisableDateTime pauseDate,
            TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
        {
            var testData = context.Get<TestData>();

            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithMathsAndEnglish(me => me.WithCourseDetails(startDate.Value, endDate.Value, course, amount, learnAimRef)
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

            learnerDataBuilder.WithEnglishAndMathsReturnFromBreakInLearning(newStartDate.Value, false,null,null,withdrawalDate.Value);

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


        [When("an English and Maths learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*), amount (.*), completion date as (.*), learning support from (.*) to (.*)")]
        public async Task AddMathsAndEnglishWithCompletionAndLearningSupport(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, string course, decimal amount, TokenisableDateTime completionDate,
            TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
        {
            var testData = context.Get<TestData>();

            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithMathsAndEnglish(me => me.WithCourseDetails(startDate.Value, endDate.Value, course, amount, learnAimRef)
            .WithCompletionDate(completionDate.Value)
            .WithLearningSupport(learningSupportStartDate.Value, learningSupportEndDate.Value));

            testData.IsMathsAndEnglishAdded = true;
        }

        [Given("Maths and English learning is recorded from (.*) for (.*) days with learnAimRef (.*), course (.*), amount (.*) and withdrawal after (.*) days")]
        [When("Maths and English learning is recorded from (.*) for (.*) days with learnAimRef (.*), course (.*), amount (.*) and withdrawal after (.*) days")]
        public async Task AddMathsAndEnglishLearningWithWithdrawal(TokenisableDateTime startDate, int duration, string learnAimRef, string course, decimal amount, int withdrawalOnDay)
        {
            var testData = context.Get<TestData>();

            var endDate = startDate.Value.AddDays(duration - 1);
            var withdrawalDate = startDate.Value.AddDays(withdrawalOnDay - 1);

            var learnerBuilder = testData.GetLearnerDataBuilder();

            learnerBuilder.WithEnglishAndMaths(startDate.Value, endDate, course, amount, learnAimRef, withdrawalDate: withdrawalDate);

            testData.IsMathsAndEnglishAdded = true;
        }


        [When("Maths and English learning is recorded from (.*) to (.*) with learnAimRef (.*), course (.*), amount (.*) and prior learning adjustment of (.*) percent")]
        public async Task AddMathsAndEnglishLearning(TokenisableDateTime startDate, TokenisableDateTime endDate, string learnAimRef, string course, decimal amount, int? priorLearning)
        {
            var testData = context.Get<TestData>();

            var learnerBuilder = testData.GetLearnerDataBuilder();
            learnerBuilder.WithEnglishAndMaths(startDate.Value, endDate.Value, course, amount, learnAimRef, priorLearningPercentage: priorLearning);

            testData.IsMathsAndEnglishAdded = true;
        }

        [Given("Maths and English earnings are generated from periods (.*) to (.*) with instalment amount (.*) for course (.*)")]
        [When("Maths and English earnings are generated from periods (.*) to (.*) with instalment amount (.*) for course (.*)")]
        [Then("Maths and English earnings are generated from periods (.*) to (.*) with instalment amount (.*) for course (.*)")]
        public async Task VerifyMathsAndEnglishInstalmentEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
            TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount, string course)
        {
            await VerifyMathsAndEnglishEarnings(mathsAndEnglishStartPeriod, mathsAndEnglishEndPeriod, amount, course,
                true);
        }

        [When("SLD inform us that Maths and English details have changed")]
        public void SLDInformUsThatMathsAndEnglishDetailsHaveChanged()
        {
            var testData = context.Get<TestData>();

            testData.ResetLearnerDataBuilder();
        }

        [Given("Maths and English earnings are generated from periods (.*) to (.*) with regular instalment amount (.*) for course (.*)")]
        [When("Maths and English earnings are generated from periods (.*) to (.*) with regular instalment amount (.*) for course (.*)")]
        [Then("Maths and English earnings are generated from periods (.*) to (.*) with regular instalment amount (.*) for course (.*)")]
        public async Task VerifyRegularMathsAndEnglishInstalmentEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
            TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount, string course)
        {
            await VerifyMathsAndEnglishEarnings(mathsAndEnglishStartPeriod, mathsAndEnglishEndPeriod, amount, course,
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
                earningsApprenticeshipModel = earningsEntitySqlClient.GetEarningsEntityModel(context);
                return !testData.IsMathsAndEnglishAdded || earningsApprenticeshipModel.Episodes.SingleOrDefault()
                    .EarningsProfileHistory.Any();
            }, "Failed to find updated earnings entity.");

            var mathsAndEnglish = earningsApprenticeshipModel
                .Episodes
                .SingleOrDefault()
                ?.MathsAndEnglish;

            var mathAndEnglishInstalments = earningsApprenticeshipModel
                .Episodes
                .SingleOrDefault()
                ?.MathsAndEnglishInstalments;

            mathsAndEnglish.Should().BeEmpty("Expected no Maths and English earnings to be generated, but found some on the earnings apprenticeship model");
            mathAndEnglishInstalments.Should().BeEmpty("Expected no Maths and English instalments to be generated, but found some on the earnings apprenticeship model");
        }

        private async Task VerifyMathsAndEnglishEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
            TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount, string course, bool assertNoSubsequentEarningsExist, bool assertNoPriorEarningsExist = false)
        {
            var testData = context.Get<TestData>();
            EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

            await WaitHelper.WaitForIt(() =>
            {
                earningsApprenticeshipModel = earningsEntitySqlClient.GetEarningsEntityModel(context);
                return !testData.IsMathsAndEnglishAdded || earningsApprenticeshipModel.Episodes.SingleOrDefault()
                    .EarningsProfileHistory.Any();
            }, "Failed to find updated earnings entity.");

            var mathsAndEnglish = earningsApprenticeshipModel
                .Episodes
                .SingleOrDefault()
                ?.MathsAndEnglish;

            var mathsAndEnglishKey = mathsAndEnglish.FirstOrDefault(x => x.Course.Contains(course)).Key;


            var mathsAndEnglishInstalments = earningsApprenticeshipModel
                .Episodes
                .SingleOrDefault()
                ?.MathsAndEnglishInstalments.Where(x => x.MathsAndEnglishKey == mathsAndEnglishKey)
                .Where(x => x.Type == "Regular")
                .ToList();

            testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile
                .EarningsProfileId;

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

            while (mathsAndEnglishStartPeriod.Value.IsBefore(mathsAndEnglishEndPeriod.Value))
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

        [When("a Maths and English balancing earning of (.*) is generated for course (.*) for period (.*)")]
        [Then("a Maths and English balancing earning of (.*) is generated for course (.*) for period (.*)")]
        public async Task VerifyMathsAndEnglishEarnings(decimal amount, string course, TokenisablePeriod period)
        {
            var mathsAndEnglishInstalments = GetLatestMathsAndEnglishInstalmentsOfType(course, "Balancing");

            mathsAndEnglishInstalments.Should()
                .NotBeNull("No Maths and English balancing instalment data found on earnings apprenticeship model");

            mathsAndEnglishInstalments.Result.Should()
                .Contain(x =>
                    x.DeliveryPeriod == period.Value.PeriodValue &&
                    x.AcademicYear == period.Value.AcademicYear &&
                    x.Amount == amount);
        }

        [Then("Maths and English balancing earning is removed for course (.*)")]
        public void MathsAndEnglishBalancingEarningIsRemovedForCourse(string course)
        {

            var mathsAndEnglishInstalments = GetLatestMathsAndEnglishInstalmentsOfType(course, "Balancing");

            Assert.Zero(mathsAndEnglishInstalments.Result.Count, "Unexpected Balancing earning for English and Maths found! ");
        }

        [Then("Maths and English earnings for course (.*) are removed")]
        public async Task VerifyMathsAndEnglishEarningsAreRemoved(string course)
        {
            var mathsAndEnglishInstalments = GetLatestMathsAndEnglishInstalmentsOfType(course, "Regular");

            mathsAndEnglishInstalments.Result.Should().BeEmpty("Unexpected Maths and English instalment data found on earnings apprenticeship model");
        }

        private async Task<List<MathsAndEnglishInstalment>> GetLatestMathsAndEnglishInstalmentsOfType(string course, string type)
        {
            var testData = context.Get<TestData>();
            EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

            await WaitHelper.WaitForIt(() =>
            {
                earningsApprenticeshipModel = earningsEntitySqlClient.GetEarningsEntityModel(context);
                return !testData.IsMathsAndEnglishAdded
                       || earningsApprenticeshipModel.Episodes.SingleOrDefault()
                           .EarningsProfileHistory.Any();
            }, "Failed to find updated earnings entity.");

            var episode = earningsApprenticeshipModel
                .Episodes
                .SingleOrDefault();

            if (episode == null || episode.MathsAndEnglish == null)
                return new List<MathsAndEnglishInstalment>();

            var mathsAndEnglishKey = episode.MathsAndEnglish
                .FirstOrDefault(x => x.Course.Contains(course))?
                .Key;

            if (mathsAndEnglishKey == null)
                return new List<MathsAndEnglishInstalment>();

            testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile.EarningsProfileId;

            return episode.MathsAndEnglishInstalments
                .Where(x => x.MathsAndEnglishKey == mathsAndEnglishKey && x.Type == type)
                .ToList();
        }
    }
}
