using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Runtime.Intrinsics.Arm;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
internal class WithdrawApprenticeshipStepDefinitions
{
    private readonly ScenarioContext _context;
    private Helpers.Sql.Apprenticeship? apprenticeship;
    private readonly EarningsRecalculatedEventHelper _earningsRecalculatedEventHelper;
    private PaymentsGeneratedEvent paymentsGeneratedEvent;
    private EarningsApprenticeshipModel? _earningsApprenticeshipModel;
    private PaymentsApprenticeshipModel? _paymentsApprenticeshipModel; 
    private ApprenticeshipEarningsRecalculatedEvent _recalculatedEarningsEvent;
    private readonly PaymentsMessageHandler _paymentsMessageHelper;
    private readonly byte _currentCollectionPeriod;
    private readonly string _currentCollectionYear;
    private DateTime _lastDayOfLearning;

    public WithdrawApprenticeshipStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _earningsRecalculatedEventHelper = new EarningsRecalculatedEventHelper(_context);
        _paymentsMessageHelper = new PaymentsMessageHandler(context);
        _currentCollectionPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];
        _currentCollectionYear = TableExtensions.CalculateAcademicYear("0");
    }

    [When(@"the apprenticeship is withdrawn")]
    public async Task WithdrawApprenticeship()
    {
        var apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var apprenticeshipsClient = new ApprenticeshipsClient();
        var body = new WithdrawApprenticeshipRequestBody
        {
            UKPRN = apprenticeshipCreatedEvent.Episode.Ukprn,
            ULN = apprenticeshipCreatedEvent.Uln,
            Reason = "WithdrawFromBeta",
            ReasonText = "",
            LastDayOfLearning = commitmentsApprenticeshipCreatedEvent.ActualStartDate!.Value.AddDays(1),
            ProviderApprovedBy = "SystemAcceptanceTest"
        };

        await apprenticeshipsClient.WithdrawApprenticeship(body);
    }

    [When("a Withdrawal request is recorded with a reason (.*) and last day of delivery (.*)")]
    public async Task WithdrawalRequestIsRecordedWithAReasonWithdrawDuringLearningAndLastDayOfDelivery(string reason, DateTime lastDayOfDelivery)
    {
        // clear previous PaymentsGeneratedEvent before triggering the withdrawal
        PaymentsGeneratedEventHandler.ReceivedEvents.Clear();

        _lastDayOfLearning = lastDayOfDelivery;

        var apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var apprenticeshipsClient = new ApprenticeshipsClient();
        var body = new WithdrawApprenticeshipRequestBody
        {
            UKPRN = apprenticeshipCreatedEvent.Episode.Ukprn,
            ULN = apprenticeshipCreatedEvent.Uln,
            Reason = reason,
            ReasonText = "",
            LastDayOfLearning = _lastDayOfLearning,
            ProviderApprovedBy = "SystemAcceptanceTest"
        };

        await apprenticeshipsClient.WithdrawApprenticeship(body);
    }


    [Then(@"the apprenticeship is marked as withdrawn")]
    public async Task ApprenticeshipIsMarkedAsWithdrawn()
    {
        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey); 

        await WaitHelper.WaitForIt(() =>
        {
            apprenticeship = apprenticeshipSqlClient.GetApprenticeship(apprenticeshipKey);

            return apprenticeship.Episodes.First().LearningStatus == "Withdrawn";
        }, "LearningStatus did not change to 'Withdrawn' in time.");

        Assert.AreEqual(apprenticeship?.WithdrawalRequests.Count, 1);
    }

    [Then("earnings are recalculated")]
    public async Task EarningsAreRecalculated()
    {
        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);

        await _earningsRecalculatedEventHelper.ReceiveEarningsRecalculatedEvent(apprenticeshipKey);

        _recalculatedEarningsEvent = _context.Get<ApprenticeshipEarningsRecalculatedEvent>();

        _earningsApprenticeshipModel = new EarningsSqlClient().GetEarningsEntityModel(_context);
    }

    [Then("payments are recalculated")]
    public async Task PaymentsAreRecalculated()
    {
        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);

        // Receive the updated PaymentsGeneratedEvent
        await _paymentsMessageHelper.ReceivePaymentsEvent(apprenticeshipKey);

        _paymentsApprenticeshipModel = new PaymentsSqlClient().GetPaymentsModel(_context);
    }


    [Then("the expected number of earnings instalments after withdrawal are (.*)")]
    public void ExpectedNumberOfEarningsInstalmentsAfterWithdrawalIs(int expectedInstalmentsNumber)
    {
        Assert.AreEqual(expectedInstalmentsNumber, _recalculatedEarningsEvent.DeliveryPeriods.Count, "Unexpected number of instalments in earnings recalculated event");

        var actualInstalmentsNumber = _earningsApprenticeshipModel?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.Count ?? 0;
        Assert.AreEqual(expectedInstalmentsNumber, actualInstalmentsNumber, "Unexpected number of instalments after withdrawal has been recorded in earnings db!");
    }

    [Then("the earnings after the delivery period (.*) and academic year (.*) are soft deleted")]
    public void EarningsAfterTheDeliveryPeriodAndAcademicYearAreSoftDeleted(string deliveryPeriod, string academicYear)
    {
        if (deliveryPeriod != null && academicYear != null)
        {
            bool isValidRecalculatedEarnings = _recalculatedEarningsEvent.DeliveryPeriods?
                .All(Dp => Dp.AcademicYear < Convert.ToInt16(academicYear) 
                || (Dp.AcademicYear == Convert.ToInt16(academicYear) && Dp.Period <= Convert.ToInt16(deliveryPeriod))) ?? true;

            Assert.IsTrue(isValidRecalculatedEarnings, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear} in recalculated earnings event.");


            bool isValidEarningInDb = _earningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?
               .All(i => i.AcademicYear < Convert.ToInt16(academicYear) 
               || (i.AcademicYear == Convert.ToInt16(academicYear) && i.DeliveryPeriod <= Convert.ToInt16(deliveryPeriod))) ?? true;

            Assert.IsTrue(isValidEarningInDb, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear} in earnings db.");
        }
    }

    [Then("new payments with amount (.*) are marked as Not sent for payment and clawed back")]
    public void NewPaymentsWithAmountSinceDeliveryPeriodAndAcademicYearAreMarkedAsNotSentForPaymentClawedBack(decimal amount)
    {
        var earningsGeneratedEvent = _context.Get<EarningsGeneratedEvent>();

        var lastDayOfLearningPeriod = TableExtensions.Period[_lastDayOfLearning.ToString("MMMM")];
        var lastDayOfLearningAcademicYear = short.Parse(TableExtensions.CalculateAcademicYear("0", _lastDayOfLearning));

        //var startDatePeriod = TableExtensions.Period[earningsGeneratedEvent.StartDate.ToString("MMMM")];
        var startDateAcademicYear = short.Parse(TableExtensions.CalculateAcademicYear("0", earningsGeneratedEvent.StartDate));

        var firstDeliveryPeriod = startDateAcademicYear < short.Parse(_currentCollectionYear) ? TableExtensions.Period["August"] : lastDayOfLearningPeriod;

           var expectedPaymentPeriods = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
           new Period(short.Parse(_currentCollectionYear), firstDeliveryPeriod),
           new Period(short.Parse(_currentCollectionYear), _currentCollectionPeriod),
           new PaymentExpectation
           {
               Amount = -amount,
               SentForPayment = false
           });

            paymentsGeneratedEvent = _context.Get<PaymentsGeneratedEvent>();

            // Validate PaymentsGenerateEvent & Payments Entity
            expectedPaymentPeriods.AssertAgainstEventPayments(paymentsGeneratedEvent.Payments);

            expectedPaymentPeriods.AssertAgainstEntityArray(_paymentsApprenticeshipModel.Payments);
    }

    [Then("all the payments from delivery periods after last payment made are deleted")]
    public void ThenAllThePaymentsFromDeliveryPeriodsAfterLastPaymentMadeAreDeleted()
    {
        bool futurePaymentsDeletedFromEvent = paymentsGeneratedEvent.Payments
        .All(x => x.AcademicYear < short.Parse(_currentCollectionYear)
            || (x.AcademicYear == Convert.ToInt16(_currentCollectionYear) && x.DeliveryPeriod <= Convert.ToInt16(_currentCollectionPeriod)));

        Assert.IsTrue(futurePaymentsDeletedFromEvent, $"Some instalments have a delivery period > {_currentCollectionPeriod} and academic year > {_currentCollectionYear} in recalculated payments event.");

        bool isValidPaymentsInDb = _paymentsApprenticeshipModel.Payments.All(i => i.AcademicYear < Convert.ToInt16(_currentCollectionYear)
               || (i.AcademicYear == Convert.ToInt16(_currentCollectionYear) && i.DeliveryPeriod <= Convert.ToInt16(_currentCollectionPeriod)));

        Assert.IsTrue(isValidPaymentsInDb, $"Some instalments have a delivery period > {_currentCollectionPeriod} and academic year > {_currentCollectionYear} in payments db.");

    }
}
