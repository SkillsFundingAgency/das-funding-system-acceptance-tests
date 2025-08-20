using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class MathAndEnglishAssertionsStepDefinitions(
        ScenarioContext context,
        EarningsSqlClient earningsEntitySqlClient,
        LearnerDataOuterApiHelper learnerDataOuterApiHelper)
    {
        [When("Maths and English learning is recorded from (.*) to (.*) with course (.*) and amount (.*)")]
        public async Task AddMathsAndEnglishLearning(TokenisableDateTime startDate, TokenisableDateTime endDate, string course, decimal amount)
        {
            var testData = context.Get<TestData>();

            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder.WithMathsAndEnglish(startDate.Value, endDate.Value, course, amount);
            
            testData.IsMathsAndEnglishAdded = true;
        }

        [When("Maths and English learning is recorded from (.*) to (.*) with course (.*), amount (.*) and completion on (.*)")]
        public async Task AddMathsAndEnglishLearningWithCompletion(TokenisableDateTime startDate, TokenisableDateTime endDate, string course, decimal amount, TokenisableDateTime completionDate)
        {
            var testData = context.Get<TestData>();

            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithMathsAndEnglish(startDate.Value, endDate.Value, course, amount, completionDate: completionDate.Value);
            
            testData.IsMathsAndEnglishAdded = true;
        }

        [When("a Maths and English learning is recorded from (.*) to (.*) with course (.*) and amount (.*) and learning support from (.*) to (.*)")]
        public async Task AddMathsAndEnglishLearningSupport(TokenisableDateTime startDate, TokenisableDateTime endDate, string course, decimal amount,
            TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
        {
            var testData = context.Get<TestData>();

            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithMathsAndEnglish(me => me.WithCourseDetails(startDate.Value, endDate.Value, course, amount)
                        .WithLearningSupport(learningSupportStartDate.Value, learningSupportEndDate.Value));

            testData.IsMathsAndEnglishAdded = true;
        }

        [When("Maths and English learning is recorded from (.*) for (.*) days with course (.*), amount (.*) and withdrawal after (.*) days")]
        public async Task AddMathsAndEnglishLearningWithWithdrawal(TokenisableDateTime startDate, int duration, string course, decimal amount, int withdrawalOnDay)
        {
            var testData = context.Get<TestData>();

            var endDate = startDate.Value.AddDays(duration - 1);
            var withdrawalDate = startDate.Value.AddDays(withdrawalOnDay - 1);

            var learnerBuilder = testData.GetLearnerDataBuilder();

            learnerBuilder.WithMathsAndEnglish(startDate.Value, endDate, course, amount, withdrawalDate: withdrawalDate);

            testData.IsMathsAndEnglishAdded = true;
        }


        [When("Maths and English learning is recorded from (.*) to (.*) with course (.*), amount (.*) and prior learning adjustment of (.*) percent")]
        public async Task AddMathsAndEnglishLearning(TokenisableDateTime startDate, TokenisableDateTime endDate, string course, decimal amount, int? priorLearning)
        {
            var testData = context.Get<TestData>();

            var learnerBuilder = testData.GetLearnerDataBuilder();
            learnerBuilder.WithMathsAndEnglish(startDate.Value, endDate.Value, course, amount, priorLearningPercentage: priorLearning);

            testData.IsMathsAndEnglishAdded = true;
        }

        [When("the first course is recorded from (.*) to (.*) with course (.*) and amount (.*) and the second course from (.*) to (.*) with course (.*) and amount (.*)")]
        public async Task AddMultipleMathsAndEnglishLearnings(TokenisableDateTime course1StartDate, TokenisableDateTime course1EndDate, string course1Name, decimal course1Amount,
            TokenisableDateTime course2StartDate, TokenisableDateTime course2EndDate, string course2Name, decimal course2Amount)
        {
            var testData = context.Get<TestData>();

            var learnerBuilder = testData.GetLearnerDataBuilder();
            
            learnerBuilder
                .WithMathsAndEnglish(b => b.WithCourseDetails(course1StartDate.Value, course1EndDate.Value, course1Name, course1Amount))
                .WithMathsAndEnglish(b => b.WithCourseDetails(course2StartDate.Value, course2EndDate.Value, course2Name, course2Amount));

            testData.IsMathsAndEnglishAdded = true;
        }

        [When(@"SLD submit updated learners details")]
        public async Task WhenSLDSubmitUpdatedLearnersDetails()
        {
            var testData = context.Get<TestData>();

            if (testData.LearnerDataBuilder == null)
            {
                throw new InvalidOperationException(
                    "No learner data builder has been stored; cannot build or submit learner data");
            }

            var learnerData =  testData.LearnerDataBuilder.Build();
                
            await learnerDataOuterApiHelper.UpdateLearning(testData.LearningKey, learnerData);
        }


        [Then("Maths and English earnings are generated from periods (.*) to (.*) with instalment amount (.*) for course (.*)")]
        public async Task VerifyMathsAndEnglishInstalmentEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
            TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount, string course)
        {
            await VerifyMathsAndEnglishEarnings(mathsAndEnglishStartPeriod, mathsAndEnglishEndPeriod, amount, course,
                true);
        }

        [Then("Maths and English earnings are generated from periods (.*) to (.*) with regular instalment amount (.*) for course (.*)")]
        public async Task VerifyRegularMathsAndEnglishInstalmentEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
            TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount, string course)
        {
            await VerifyMathsAndEnglishEarnings(mathsAndEnglishStartPeriod, mathsAndEnglishEndPeriod, amount, course,
                false);
        }

        private async Task VerifyMathsAndEnglishEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
            TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount, string course, bool assertNoSubsequentEarningsExist)
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
                .ToList();

            testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile
                .EarningsProfileId;

            mathsAndEnglishInstalments.Should()
                .NotBeNull("No Maths and English instalment data found on earnings apprenticeship model");

            mathsAndEnglishInstalments.Should().NotContain(x =>
                    new Period(x.AcademicYear, x.DeliveryPeriod).IsBefore(mathsAndEnglishStartPeriod.Value),
                $"Expected no Maths and English earnings before {mathsAndEnglishStartPeriod.Value.ToCollectionPeriodString()}");

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

        [Then("a Maths and English earning of (.*) is generated for course (.*) for period (.*)")]
        public async Task VerifyMathsAndEnglishEarnings(decimal amount, string course, TokenisablePeriod period)
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
                .ToList();

            testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile
                .EarningsProfileId;

            mathsAndEnglishInstalments.Should()
                .NotBeNull("No Maths and English instalment data found on earnings apprenticeship model");

            mathsAndEnglishInstalments.Should()
                .Contain(x =>
                    x.DeliveryPeriod == period.Value.PeriodValue &&
                    x.AcademicYear == period.Value.AcademicYear &&
                    x.Amount == amount);
        }


        [Then("Maths and English earnings for course (.*) are zero")]
        public async Task VerifyMathsAndEnglishEarnings(string course)
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
                .ToList();

            testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile
                .EarningsProfileId;

            mathsAndEnglishInstalments.Should().BeEmpty("Unexpected Maths and English instalment data found on earnings apprenticeship model");
        }
    }
}
