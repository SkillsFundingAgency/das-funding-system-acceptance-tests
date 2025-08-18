using AutoFixture;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class MathAndEnglishAssertionsStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly EarningsSqlClient _earningsSqlClient;
        private readonly LearnerDataOuterApiHelper _learnerDataOuterApiHelper;

        public MathAndEnglishAssertionsStepDefinitions(ScenarioContext context,
            EarningsSqlClient earningsEntitySqlClient, LearnerDataOuterApiHelper learnerDataOuterApiHelper)
        {
            _context = context;
            _earningsSqlClient = earningsEntitySqlClient;
            _learnerDataOuterApiHelper = learnerDataOuterApiHelper;
        }

        [When("Maths and English learning is recorded from (.*) to (.*) with course (.*) and amount (.*)")]
        public async Task AddMathsAndEnglishLearning(TokenisableDateTime StartDate, TokenisableDateTime EndDate,
            string course, decimal amount)
        {
            var testData = _context.Get<TestData>();
            var helper = new EarningsInnerApiHelper();
            await helper.SetMathAndEnglishLearning(testData.LearningKey,
            [
                new EarningsInnerApiClient.MathAndEnglishDetails
                    { StartDate = StartDate.Value, EndDate = EndDate.Value, Amount = amount, Course = course }
            ]);

            testData.IsMathsAndEnglishAdded = true;
        }

        [When("Maths and English learning is recorded from (.*) to (.*) with course (.*), amount (.*) and completion on (.*)")]
        public async Task AddMathsAndEnglishLearningWithCompletion(TokenisableDateTime StartDate, TokenisableDateTime EndDate,
            string course, decimal amount, TokenisableDateTime completionDate)
        {
            var testData = _context.Get<TestData>();

            await _learnerDataOuterApiHelper.AddMathsAndEnglish(testData.LearningKey,
                new LearnerDataOuterApiClient.MathsAndEnglish
                {
                    Amount = amount,
                    CompletionDate = completionDate.Value,
                    Course = course,
                    StartDate = StartDate.Value,
                    PlannedEndDate = EndDate.Value
                });

            testData.IsMathsAndEnglishAdded = true;
        }

        [When("a Maths and English learning is recorded from (.*) to (.*) with course (.*) and amount (.*) and learning support from (.*) to (.*)")]
        public async Task AddMathsAndEnglishLearningSupport(TokenisableDateTime StartDate, TokenisableDateTime EndDate,
            string course, decimal amount, TokenisableDateTime LearningSupportStartDate, TokenisableDateTime LearningSupportEndDate)
        {
            var testData = _context.Get<TestData>();

            await _learnerDataOuterApiHelper.AddMathsAndEnglish(testData.LearningKey,
                new LearnerDataOuterApiClient.MathsAndEnglish
                {
                    Amount = amount,
                    Course = course,
                    StartDate = StartDate.Value,
                    PlannedEndDate = EndDate.Value,
                    LearningSupport = new List<LearnerDataOuterApiClient.LearningSupport>
                    {
                        new LearnerDataOuterApiClient.LearningSupport {
                        StartDate = LearningSupportStartDate.Value,
                        EndDate = LearningSupportEndDate.Value
                    }
                    }
                });

            testData.IsMathsAndEnglishAdded = true;
        }

        [When("Maths and English learning is recorded from (.*) for (.*) days with course (.*), amount (.*) and withdrawal after (.*) days")]
        public async Task AddMathsAndEnglishLearningWithWithdrawal(TokenisableDateTime startDate, int duration,
            string course, decimal amount, int withdrawalOnDay)
        {
            var endDate = startDate.Value.AddDays(duration - 1);
            var withdrawalDate = startDate.Value.AddDays(withdrawalOnDay - 1);

            var testData = _context.Get<TestData>();

            await _learnerDataOuterApiHelper.AddMathsAndEnglish(testData.LearningKey,
                new LearnerDataOuterApiClient.MathsAndEnglish
                {
                    Amount = amount,
                    Course = course,
                    StartDate = startDate.Value,
                    PlannedEndDate = endDate,
                    WithdrawalDate = withdrawalDate
                });

            testData.IsMathsAndEnglishAdded = true;
        }

        [When("Maths and English learning is recorded from (.*) to (.*) with course (.*), amount (.*) and prior learning adjustment of (.*) percent")]
        public async Task AddMathsAndEnglishLearning(TokenisableDateTime startDate, TokenisableDateTime endDate,
            string course, decimal amount, int? priorLearning)
        {
            var testData = _context.Get<TestData>();

            await _learnerDataOuterApiHelper.AddMathsAndEnglish(testData.LearningKey,
                new LearnerDataOuterApiClient.MathsAndEnglish
                {
                    Amount = amount,
                    Course = course,
                    StartDate = startDate.Value,
                    PlannedEndDate = endDate.Value,
                    PriorLearningPercentage = priorLearning
                });

            testData.IsMathsAndEnglishAdded = true;
        }

        [When("the first course is recorded from (.*) to (.*) with course (.*) and amount (.*) and the second course from (.*) to (.*) with course (.*) and amount (.*)")]
        public async Task AddMultipleMathsAndEnglishLearnings(TokenisableDateTime Course1StartDate, TokenisableDateTime Course1EndDate,
            string Course1Name, decimal Course1Amount, TokenisableDateTime Course2StartDate, TokenisableDateTime Course2EndDate, string Course2Name, decimal Course2Amount)
        {
            var testData = _context.Get<TestData>();
            var helper = new EarningsInnerApiHelper();
            await helper.SetMathAndEnglishLearning(testData.LearningKey,
            [
                new EarningsInnerApiClient.MathAndEnglishDetails
                    { StartDate = Course1StartDate.Value, EndDate = Course1EndDate.Value, Amount = Course1Amount, Course = Course1Name },
                new  EarningsInnerApiClient.MathAndEnglishDetails
                    { StartDate = Course2StartDate.Value, EndDate = Course2EndDate.Value, Amount = Course2Amount, Course = Course2Name }
            ]
            );

            testData.IsMathsAndEnglishAdded = true;
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
            var testData = _context.Get<TestData>();
            EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

            await WaitHelper.WaitForIt(() =>
            {
                earningsApprenticeshipModel = _earningsSqlClient.GetEarningsEntityModel(_context);
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
            var testData = _context.Get<TestData>();
            EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

            await WaitHelper.WaitForIt(() =>
            {
                earningsApprenticeshipModel = _earningsSqlClient.GetEarningsEntityModel(_context);
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
            var testData = _context.Get<TestData>();
            EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

            await WaitHelper.WaitForIt(() =>
            {
                earningsApprenticeshipModel = _earningsSqlClient.GetEarningsEntityModel(_context);
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
