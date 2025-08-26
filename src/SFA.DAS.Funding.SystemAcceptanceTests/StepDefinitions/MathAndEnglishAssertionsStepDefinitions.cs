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

        [Given("Maths and English learning is recorded from (.*) for (.*) days with course (.*), amount (.*) and withdrawal after (.*) days")]
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
