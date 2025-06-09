using NUnit.Framework.Interfaces;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
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
        private readonly PaymentsSqlClient _paymentsSqlClient;


        public MathAndEnglishAssertionsStepDefinitions(ScenarioContext context,
            EarningsSqlClient earningsEntitySqlClient, PaymentsSqlClient paymentsEntitySqlClient)
        {
            _context = context;
            _earningsSqlClient = earningsEntitySqlClient;
            _paymentsSqlClient = paymentsEntitySqlClient;
        }

        [When("Maths and English learning is recorded from (.*) to (.*) with course (.*) and amount (.*)")]
        public async Task AddMathsAndEnglishLearning(TokenisableDateTime StartDate, TokenisableDateTime EndDate,
            string course, decimal amount)
        {
            var testData = _context.Get<TestData>();
            PaymentsGeneratedEventHandler.Clear(x => x.ApprenticeshipKey == testData.ApprenticeshipKey);
            var helper = new EarningsInnerApiHelper();
            await helper.SetMathAndEnglishLearning(testData.ApprenticeshipKey,
            [
                new EarningsInnerApiClient.MathAndEnglishDetails
                    { StartDate = StartDate.Value, EndDate = EndDate.Value, Amount = amount, Course = course }
            ]);

            testData.IsMathsAndEnglishAdded = true;
        }

        [When("the first course is recorded from (.*) to (.*) with course (.*) and amount (.*) and the second course from (.*) to (.*) with course (.*) and amount (.*)")]
        public async Task AddMultipleMathsAndEnglishLearnings(TokenisableDateTime Course1StartDate, TokenisableDateTime Course1EndDate,
            string Course1Name, decimal Course1Amount, TokenisableDateTime Course2StartDate, TokenisableDateTime Course2EndDate, string Course2Name, decimal Course2Amount)
        {
            var testData = _context.Get<TestData>();
            PaymentsGeneratedEventHandler.Clear(x => x.ApprenticeshipKey == testData.ApprenticeshipKey);
            var helper = new EarningsInnerApiHelper();
            await helper.SetMathAndEnglishLearning(testData.ApprenticeshipKey,
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
        public async Task VerifyMathsAndEnglishEarnings(TokenisablePeriod mathsAndEnglishStartPeriod,
            TokenisablePeriod mathsAndEnglishEndPeriod, decimal Amount, string course)
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

            var mathsAndEnglishKey = mathsAndEnglish.Where(x => x.Course.Contains(course)).FirstOrDefault().Key;

            var mathsAndEnglishInstalments = earningsApprenticeshipModel
                .Episodes
                .SingleOrDefault()
                ?.MathsAndEnglishInstalments.Where(x => x.MathsAndEnglishKey == mathsAndEnglishKey);

            testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile
                .EarningsProfileId;

            mathsAndEnglishInstalments.Should()
                .NotBeNull("No Maths and English instalment data found on earnings apprenticeship model");

            while (mathsAndEnglishStartPeriod.Value.AcademicYear < mathsAndEnglishEndPeriod.Value.AcademicYear ||
                   (mathsAndEnglishStartPeriod.Value.AcademicYear == mathsAndEnglishEndPeriod.Value.AcademicYear &&
                    mathsAndEnglishStartPeriod.Value.PeriodValue <= mathsAndEnglishEndPeriod.Value.PeriodValue))
            {
                mathsAndEnglishInstalments.Should().Contain(x =>
                        x.MathsAndEnglishKey == mathsAndEnglishKey
                        && x.Amount == Amount
                        && x.AcademicYear == mathsAndEnglishStartPeriod.Value.AcademicYear
                        && x.DeliveryPeriod == mathsAndEnglishStartPeriod.Value.PeriodValue,
                    $"Expected Maths and English earning for {mathsAndEnglishStartPeriod.Value.ToCollectionPeriodString()}");

                mathsAndEnglishStartPeriod.Value = mathsAndEnglishStartPeriod.Value.GetNextPeriod();
            }
        }

        [Then(@"Maths and English payments are generated from periods (.*) to (.*) with amount (.*)")]
        public async Task VerifyMathsAndEnglishPayments(TokenisablePeriod mathsAndEnglishStartPeriod,
            TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount)
        {
            var testData = _context.Get<TestData>();
            PaymentsApprenticeshipModel? paymentsApprenticeshipModel = null;

            await WaitHelper.WaitForIt(() =>
            {
                paymentsApprenticeshipModel = _paymentsSqlClient.GetPaymentsModel(_context);
                if (paymentsApprenticeshipModel == null) return false;
                return paymentsApprenticeshipModel.Earnings.Any(x => x.EarningsProfileId == testData.EarningsProfileId);
            }, "Failed to find updated payments entity.");

            while (mathsAndEnglishStartPeriod.Value.AcademicYear < mathsAndEnglishEndPeriod.Value.AcademicYear ||
                   (mathsAndEnglishStartPeriod.Value.AcademicYear == mathsAndEnglishEndPeriod.Value.AcademicYear &&
                    mathsAndEnglishStartPeriod.Value.PeriodValue <= mathsAndEnglishEndPeriod.Value.PeriodValue))
            {
                testData.PaymentsGeneratedEvent.Payments.Should().Contain(x =>
                        x.PaymentType == AdditionalPaymentType.MathsAndEnglish.ToString()
                        && x.Amount == amount
                        && x.AcademicYear == mathsAndEnglishStartPeriod.Value.AcademicYear
                        && x.DeliveryPeriod == mathsAndEnglishStartPeriod.Value.PeriodValue,
                    $"Expected Maths and English payments for {mathsAndEnglishStartPeriod.Value.ToCollectionPeriodString()}");

                mathsAndEnglishStartPeriod.Value = mathsAndEnglishStartPeriod.Value.GetNextPeriod();
            }
        }

        [Then("no Maths and English payments are generated")]
        public async Task NoMathsAndEnglishPaymentsAreGenerated()
        {
            var testData = _context.Get<TestData>();

            testData.PaymentsGeneratedEvent.Payments
                .Where(x => x.PaymentType == "MathsAndEnglish")
                .Should()
                .BeEmpty("no MathsAndEnglish payments should be present");
        }


        [When(@"the payments event is disregarded")]
        public async Task WhenThePaymentsEventIsDisregarded()
        {
            var testData = _context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
                {
                    var e = PaymentsGeneratedEventHandler.GetMessage(x => x.ApprenticeshipKey == testData.ApprenticeshipKey);
                    return e != null;
                },
                $"Failed to find published event in Payments");

            PaymentsGeneratedEventHandler.Clear(x => x.ApprenticeshipKey == testData.ApprenticeshipKey);
        }
    }
}
