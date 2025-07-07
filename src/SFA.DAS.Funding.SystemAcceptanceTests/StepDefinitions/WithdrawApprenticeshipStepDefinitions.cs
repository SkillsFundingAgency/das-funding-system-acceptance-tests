using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
internal class WithdrawApprenticeshipStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly LearningClient _apprenticeshipsClient;
    private readonly LearningSqlClient _apprenticeshipSqlClient;
    private readonly EarningsSqlClient _earningsSqlClient;
    private readonly PaymentsSqlClient _paymentsSqlClient;

    public WithdrawApprenticeshipStepDefinitions(
        ScenarioContext context,
        LearningClient apprenticeshipsClient,
        LearningSqlClient apprenticeshipSqlClient,
        EarningsSqlClient earningsSqlClient,
        PaymentsSqlClient paymentsSqlClient)
    {
        _context = context;
        _apprenticeshipsClient = apprenticeshipsClient;
        _apprenticeshipSqlClient = apprenticeshipSqlClient;
        _earningsSqlClient = earningsSqlClient;
        _paymentsSqlClient = paymentsSqlClient;
    }

    [When(@"the apprenticeship is withdrawn")]
    public async Task WithdrawApprenticeship()
    {
        var testData = _context.Get<TestData>();

        var body = new WithdrawLearningRequestBody
        {
            UKPRN = testData.LearningCreatedEvent.Episode.Ukprn,
            ULN = testData.LearningCreatedEvent.Uln,
            Reason = "WithdrawFromBeta",
            ReasonText = "",
            LastDayOfLearning = testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate!.Value.AddDays(1),
            ProviderApprovedBy = "SystemAcceptanceTest"
        };

        await _apprenticeshipsClient.WithdrawLearning(body);
    }

    [When("a Withdrawal request is recorded with a reason (.*) and last day of delivery (.*)")]
    public async Task WithdrawalRequestIsRecordedWithAReasonWithdrawDuringLearningAndLastDayOfDelivery(string reason, TokenisableDateTime lastDayOfDelivery)
    {
        var testData = _context.Get<TestData>();

        // clear previous PaymentsGeneratedEvent before triggering the withdrawal
        PaymentsGeneratedEventHandler.Clear(x => x.ApprenticeshipKey == testData.LearningKey);

        testData.LastDayOfLearning = lastDayOfDelivery.Value;

        var body = new WithdrawLearningRequestBody
        {
            UKPRN = testData.LearningCreatedEvent.Episode.Ukprn,
            ULN = testData.LearningCreatedEvent.Uln,
            Reason = reason,
            ReasonText = "",
            LastDayOfLearning = testData.LastDayOfLearning,
            ProviderApprovedBy = "SystemAcceptanceTest"
        };

        await _apprenticeshipsClient.WithdrawLearning(body);
    }


    [Then(@"the apprenticeship is marked as withdrawn")]
    public async Task ApprenticeshipIsMarkedAsWithdrawn()
    {
        var testData = _context.Get<TestData>();
        SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql.Learning? apprenticeship = null;

        await WaitHelper.WaitForIt(() =>
        {
            apprenticeship = _apprenticeshipSqlClient.GetApprenticeship(testData.LearningKey);

            return apprenticeship.Episodes.First().LearningStatus == "Withdrawn";
        }, "LearningStatus did not change to 'Withdrawn' in time.");

        Assert.AreEqual(apprenticeship?.WithdrawalRequests.Count, 1);
    }

    [Then("earnings are recalculated")]
    public async Task EarningsAreRecalculated()
    {
        var testData = _context.Get<TestData>();
        await _context.ReceiveEarningsRecalculatedEvent(testData.LearningKey);
        testData.EarningsApprenticeshipModel = _earningsSqlClient.GetEarningsEntityModel(_context);
    }

    [Then("payments are recalculated")]
    public async Task PaymentsAreRecalculated()
    {
        var testData = _context.Get<TestData>();
        testData.PaymentsApprenticeshipModel = _paymentsSqlClient.GetPaymentsModel(_context);
    }


    [Then("the expected number of earnings instalments after withdrawal are (.*)")]
    public void ExpectedNumberOfEarningsInstalmentsAfterWithdrawalIs(int expectedInstalmentsNumber)
    {
        var testData = _context.Get<TestData>();

        Assert.AreEqual(expectedInstalmentsNumber, testData.ApprenticeshipEarningsRecalculatedEvent.DeliveryPeriods.Count, "Unexpected number of instalments in earnings recalculated event");

        var actualInstalmentsNumber = testData.EarningsApprenticeshipModel?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.Count ?? 0;
        Assert.AreEqual(expectedInstalmentsNumber, actualInstalmentsNumber, "Unexpected number of instalments after withdrawal has been recorded in earnings db!");
    }

    [Then("the earnings after the delivery period (.*) and academic year (.*) are soft deleted")]
    public void EarningsAfterTheDeliveryPeriodAndAcademicYearAreSoftDeleted(string deliveryPeriod, string academicYear)
    {
        var testData = _context.Get<TestData>();

        if (deliveryPeriod != null && academicYear != null)
        {
            bool isValidRecalculatedEarnings = testData.ApprenticeshipEarningsRecalculatedEvent.DeliveryPeriods?
                .All(Dp => Dp.AcademicYear < Convert.ToInt16(academicYear) 
                || (Dp.AcademicYear == Convert.ToInt16(academicYear) && Dp.Period <= Convert.ToInt16(deliveryPeriod))) ?? true;

            Assert.IsTrue(isValidRecalculatedEarnings, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear} in recalculated earnings event.");


            bool isValidEarningInDb = testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?
               .All(i => i.AcademicYear < Convert.ToInt16(academicYear) 
               || (i.AcademicYear == Convert.ToInt16(academicYear) && i.DeliveryPeriod <= Convert.ToInt16(deliveryPeriod))) ?? true;

            Assert.IsTrue(isValidEarningInDb, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear} in earnings db.");
        }
    }

    [Then("new payments with amount (.*) are marked as Not sent for payment and clawed back")]
    public void NewPaymentsWithAmountSinceDeliveryPeriodAndAcademicYearAreMarkedAsNotSentForPaymentClawedBack(decimal amount)
    {
        var testData = _context.Get<TestData>();

        var lastDayOfLearningPeriod = TableExtensions.Period[testData.LastDayOfLearning.ToString("MMMM")];
        var lastDayOfLearningAcademicYear = short.Parse(TableExtensions.CalculateAcademicYear("0", testData.LastDayOfLearning));

        //var startDatePeriod = TableExtensions.Period[earningsGeneratedEvent.StartDate.ToString("MMMM")];
        var startDateAcademicYear = short.Parse(TableExtensions.CalculateAcademicYear("0", testData.EarningsGeneratedEvent.StartDate));

        var firstDeliveryPeriod = startDateAcademicYear < short.Parse(testData.CurrentCollectionYear) ? TableExtensions.Period["August"] : lastDayOfLearningPeriod;

           var expectedPaymentPeriods = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
           new Period(short.Parse(testData.CurrentCollectionYear), firstDeliveryPeriod),
           new Period(short.Parse(testData.CurrentCollectionYear), testData.CurrentCollectionPeriod),
           new PaymentExpectation
           {
               Amount = -amount,
               SentForPayment = false
           });

            // Validate PaymentsGenerateEvent & Payments Entity
            expectedPaymentPeriods.AssertAgainstEventPayments(testData.PaymentsGeneratedEvent.Payments);

            expectedPaymentPeriods.AssertAgainstEntityArray(testData.PaymentsApprenticeshipModel.Payments);
    }

    [Then("all the payments from delivery periods after last payment made are deleted")]
    public void ThenAllThePaymentsFromDeliveryPeriodsAfterLastPaymentMadeAreDeleted()
    {
        var testData = _context.Get<TestData>();
        bool futurePaymentsDeletedFromEvent = testData.PaymentsGeneratedEvent.Payments
        .All(x => x.AcademicYear < short.Parse(testData.CurrentCollectionYear)
            || (x.AcademicYear == Convert.ToInt16(testData.CurrentCollectionYear) && x.DeliveryPeriod <= Convert.ToInt16(testData.CurrentCollectionPeriod)));

        Assert.IsTrue(futurePaymentsDeletedFromEvent, $"Some instalments have a delivery period > {testData.CurrentCollectionPeriod} and academic year > {testData.CurrentCollectionYear} in recalculated payments event.");

        bool isValidPaymentsInDb = testData.PaymentsApprenticeshipModel.Payments.All(i => i.AcademicYear < Convert.ToInt16(testData.CurrentCollectionYear)
               || (i.AcademicYear == Convert.ToInt16(testData.CurrentCollectionYear) && i.DeliveryPeriod <= Convert.ToInt16(testData.CurrentCollectionPeriod)));

        Assert.IsTrue(isValidPaymentsInDb, $"Some instalments have a delivery period > {testData.CurrentCollectionPeriod} and academic year > {testData.CurrentCollectionYear} in payments db.");

    }
}
