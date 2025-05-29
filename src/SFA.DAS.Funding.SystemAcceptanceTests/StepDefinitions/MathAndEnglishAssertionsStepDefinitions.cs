using SFA.DAS.Apprenticeships.Types;
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


        public MathAndEnglishAssertionsStepDefinitions(ScenarioContext context, EarningsSqlClient earningsEntitySqlClient, PaymentsSqlClient paymentsEntitySqlClient)
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
            PaymentsGeneratedEventHandler.ReceivedEvents.Clear();
            var helper = new EarningsInnerApiHelper();
            await helper.SetMathAndEnglishLearning(testData.ApprenticeshipKey,
            [
                new EarningsInnerApiClient.MathAndEnglishDetails
                    { StartDate = StartDate.Value, EndDate = EndDate.Value, Amount = amount, Course = course }
            ]);
        
            testData.IsMathsAndEnglishAdded = true;
        }

        [Then("Maths and English earnings are generated from periods (.*) to (.*) with instalment amount (.*)")]
        public async Task VerifyMathsAndEnglishEarnings(TokenisablePeriod mathsAndEnglishStartPeriod, TokenisablePeriod mathsAndEnglishEndPeriod, decimal Amount)
        {
            var testData = _context.Get<TestData>();
            EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

            await WaitHelper.WaitForIt(() =>
            {
                earningsApprenticeshipModel = _earningsSqlClient.GetEarningsEntityModel(_context);
                return !testData.IsMathsAndEnglishAdded || earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfileHistory.Any();
            }, "Failed to find updated earnings entity.");

            var mathsAndEnglishInstalments = earningsApprenticeshipModel
                .Episodes
                .SingleOrDefault()
                ?.MathsAndEnglishInstalments;

            testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile.EarningsProfileId;

            mathsAndEnglishInstalments.Should().NotBeNull("No Maths and English instalment data found on earnings apprenticeship model");

            while (mathsAndEnglishStartPeriod.Value.AcademicYear < mathsAndEnglishEndPeriod.Value.AcademicYear || mathsAndEnglishStartPeriod.Value.AcademicYear >= mathsAndEnglishEndPeriod.Value.AcademicYear && mathsAndEnglishStartPeriod.Value.PeriodValue <= mathsAndEnglishEndPeriod.Value.PeriodValue)
            {
                mathsAndEnglishInstalments.Should().Contain(x =>
                        x.Amount == Amount
                        && x.AcademicYear == mathsAndEnglishStartPeriod.Value.AcademicYear
                        && x.DeliveryPeriod == mathsAndEnglishStartPeriod.Value.PeriodValue, $"Expected Maths and English earning for {mathsAndEnglishStartPeriod.Value.ToCollectionPeriodString()}");

                mathsAndEnglishStartPeriod.Value = mathsAndEnglishStartPeriod.Value.GetNextPeriod();
            }
        }

        [Then(@"Maths and English payments are generated from periods (.*) to (.*) with amount (.*)")]
        public async Task VerifyMathsAndEnglishPayments(TokenisablePeriod mathsAndEnglishStartPeriod, TokenisablePeriod mathsAndEnglishEndPeriod, decimal amount)
        {
            var testData = _context.Get<TestData>();
            PaymentsApprenticeshipModel? paymentsApprenticeshipModel = null;

            await WaitHelper.WaitForIt(() =>
            {
                paymentsApprenticeshipModel = _paymentsSqlClient.GetPaymentsModel(_context);
                return paymentsApprenticeshipModel.Earnings.Any(x => x.EarningsProfileId == testData.EarningsProfileId);
            }, "Failed to find updated payments entity.");

            await _context.ReceivePaymentsEvent(testData.ApprenticeshipKey);


            while (mathsAndEnglishStartPeriod.Value.AcademicYear < mathsAndEnglishEndPeriod.Value.AcademicYear || mathsAndEnglishStartPeriod.Value.AcademicYear >= mathsAndEnglishEndPeriod.Value.AcademicYear && mathsAndEnglishStartPeriod.Value.PeriodValue <= mathsAndEnglishEndPeriod.Value.PeriodValue)
            {
                testData.PaymentsGeneratedEvent.Payments.Should().Contain(x =>
                        x.PaymentType == AdditionalPaymentType.MathsAndEnglish.ToString()
                        && x.Amount == amount
                        && x.AcademicYear == mathsAndEnglishStartPeriod.Value.AcademicYear
                        && x.DeliveryPeriod == mathsAndEnglishStartPeriod.Value.PeriodValue, $"Expected Maths and English payments for {mathsAndEnglishStartPeriod.Value.ToCollectionPeriodString()}");

                mathsAndEnglishStartPeriod.Value = mathsAndEnglishStartPeriod.Value.GetNextPeriod();
            }
        }
    }
}
