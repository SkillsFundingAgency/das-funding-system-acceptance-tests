using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class CalculateUnfundedPaymentsStepDefinitions
{
    private readonly ScenarioContext _context;
    private List<FinalisedOnProgammeLearningPaymentEvent> _finalisedPaymentsList;
    private List<TestSupport.Payments> _paymentEntity;
    private readonly PaymentsFunctionsClient _paymentsFunctionsClient;

    public CalculateUnfundedPaymentsStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _paymentsFunctionsClient = new PaymentsFunctionsClient();
    }

    [Given(@"the Unfunded Payments for the remainder of the apprenticeship are determined")]
    [When(@"the Unfunded Payments for the remainder of the apprenticeship are determined")]
    public async Task UnfundedPaymentsForTheRemainderOfTheApprenticeshipAreDetermined()
    {
        var testData = _context.Get<TestData>();
        await _context.ReceivePaymentsEvent(testData.ApprenticeshipKey);
    }

    [Then(@"the Unfunded Payments for every earning is created")]
    public void UnfundedPaymentsForEveryEarningIsCreatedInTheFollowingMonth(Table table)
    {
        var testData = _context.Get<TestData>();
        var payments = testData.PaymentsGeneratedEvent.Payments;
        payments.ShouldHaveCorrectPaymentsGenerated(table.ToExpectedPayments());
    }

    [Then(@"Unfunded Payments for the apprenticeship including rollup payments are calculated as below")]
    public void UnfundedPaymentsForTheApprenticeshipIncludingRollupPaymentsAreCalculated(Table table)
    {
        var testData = _context.Get<TestData>();
        var payments = testData.PaymentsGeneratedEvent.Payments;
        payments.ShouldHaveCorrectPaymentsGenerated(table.ToExpectedRollupPayments());
    }

    [Then(@"the newly calculated Unfunded Payments are marked as not sent to payments BAU")]
    public void NewlyCalculatedUnfundedPaymentsAreMarkedAsNotSentToPaymentsBAU()
    {
        var apiClient = new PaymentsSqlClient();

        var payments = apiClient.GetPaymentsModel(_context).Payments;

        Assert.IsTrue(payments.All(x => !x.SentForPayment));
    }

    [Given(@"the user wants to process payments for the current collection Period")]
    public void UserWantsToProcessPaymentsForTheCurrentCollectionPeriod()
    {
        var testData = _context.Get<TestData>();
        testData.CurrentCollectionYear = TableExtensions.CalculateAcademicYear("0");
        testData.CurrentCollectionPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];
    }

    [When(@"the scheduler triggers Unfunded Payment processing")]
    [Then(@"the scheduler triggers Unfunded Payment processing")]
    public async Task SchedulerTriggersUnfundedPaymentProcessing()
    {
        var testData = _context.Get<TestData>();

        await _paymentsFunctionsClient.InvokeReleasePaymentsHttpTrigger(testData.CurrentCollectionPeriod,
            Convert.ToInt16(testData.CurrentCollectionYear));

        await Task.Delay(10000);
    }

    [Given(@"fire command")]
    public async Task FireCommand()
    {
        var testData = _context.Get<TestData>();
        await _paymentsFunctionsClient.InvokeReleasePaymentsHttpTrigger(testData.CurrentCollectionPeriod,
            Convert.ToInt16(testData.CurrentCollectionYear));
    }

    [When(@"the Release Payments command is published again")]
    public async Task WhenTheReleasePaymentsCommandIsPublishedAgain()
    {
        FinalisedOnProgrammeLearningPaymentEventHandler.ReceivedEvents.Clear();
        var testData = _context.Get<TestData>();

        await _paymentsFunctionsClient.InvokeReleasePaymentsHttpTrigger(testData.CurrentCollectionPeriod,
            Convert.ToInt16(testData.CurrentCollectionYear));
    }

    [When(@"the unpaid unfunded payments for the current Collection Month and (.*) rollup payments are sent to be paid")]
    public async Task UnpaidUnfundedPaymentsForTheCurrentCollectionMonthAndRollupPaymentsAreSentToBePaid(int numberOfRollupPayments)
    {
        await _context.UnpaidUnfundedPaymentsForTheCurrentCollectionMonthAndRollupPaymentsAreSentToBePaid(numberOfRollupPayments);
    }


    [Then(@"the amount of (.*) is sent to be paid for each payment in the curent Collection Month")]
    public void AmountIsSentToBePaidForEachPaymentInTheCurentCollectionMonth(decimal Amount)
    {
        var finalisedPaymentsList = _context.Get<List<FinalisedOnProgammeLearningPaymentEvent>>();
        Assert.Multiple(() =>
        {
            Assert.IsTrue(finalisedPaymentsList.All(x => x.Amount == Amount), "Incorrect Amount found in FinalisedOnProgrammeLearningPaymentEvent");
            Assert.IsTrue(finalisedPaymentsList.All(x => x.CollectionYear.ToString() == TableExtensions.CalculateAcademicYear("CurrentMonth+0")),
                "Incorrect CollectionYear found in FinalisedOnProgrammeLearningPaymentEvent");
        });
    }


    [Then(@"the relevant payments entities are marked as sent to payments BAU")]
    public void PaymentsEntitiesAreMarkedAsSentToPaymentsBAU()
    {
        var apiClient = new PaymentsSqlClient();

        _paymentEntity = apiClient.GetPaymentsModel(_context).Payments;

        var testData = _context.Get<TestData>();

        Assert.IsTrue(_paymentEntity.Where(p => p.CollectionPeriod == testData.CurrentCollectionPeriod).All(p => p.SentForPayment));
    }

    [Then(@"all payments for the following collection periods are marked as not sent to payments BAU")]
    public void AllPaymentsForTheFollowingCollectionPeriodsAreAreMarkedAsNotSentToPaymentsBAU()
    {
        var currentAcademicYear = Convert.ToInt32(TableExtensions.CalculateAcademicYear("CurrentMonth+0"));

        var testData = _context.Get<TestData>();

        Assert.IsFalse(_paymentEntity.Where(p => (p.CollectionYear > currentAcademicYear) ||
                                      (p.CollectionYear == currentAcademicYear && p.CollectionPeriod > testData.CurrentCollectionPeriod))
                                      .All(p => p.SentForPayment));
    }

    [Then(@"the unfunded payments that have already been sent to Payments BAU are not sent to be paid again")]
    public async Task ThenTheUnfundedPaymentsThatHaveAlreadyBeenSentToPaymentsBAUAreNotSentToBePaidAgain()
    {
        var testData = _context.Get<TestData>();
        await WaitHelper.WaitForUnexpected(() =>
        {
            _finalisedPaymentsList = FinalisedOnProgrammeLearningPaymentEventHandler.ReceivedEvents.Where(x => x.message.ApprenticeshipKey == testData.ApprenticeshipKey).Select(x => x.message).ToList();

            return _finalisedPaymentsList.Count != 0;
        }, "Unexpected published Finalised On Programme Learning Payment events found");
    }
}
