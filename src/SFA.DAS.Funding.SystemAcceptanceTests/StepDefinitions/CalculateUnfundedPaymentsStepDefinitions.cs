using SFA.DAS.Learning.Types;
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
    private readonly PaymentsFunctionsClient _paymentsFunctionsClient;
    private readonly PaymentsSqlClient _paymentsSqlClient;

    public CalculateUnfundedPaymentsStepDefinitions(ScenarioContext context, PaymentsFunctionsClient paymentsFunctionsClient, PaymentsSqlClient paymentsSqlClient)
    {
        _context = context;
        _paymentsFunctionsClient = paymentsFunctionsClient;
        _paymentsSqlClient = paymentsSqlClient;
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
        var payments = _paymentsSqlClient.GetPayments(_context);
        Assert.IsTrue(payments.All(x => !x.SentForPayment));
    }

    [Given(@"the user wants to process payments for the current collection Period")]
    public void UserWantsToProcessPaymentsForTheCurrentCollectionPeriod()
    {
        var testData = _context.Get<TestData>();
        testData.CurrentCollectionYear = TableExtensions.CalculateAcademicYear("0");
        testData.CurrentCollectionPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];
    }

    [When(@"the unpaid unfunded payments for the current Collection Month and (.*) rollup payments are sent to be paid")]
    public async Task UnpaidUnfundedPaymentsForTheCurrentCollectionMonthAndRollupPaymentsAreSentToBePaid(int numberOfRollupPayments)
    {
        await _context.UnpaidUnfundedPaymentsForTheCurrentCollectionMonthAndRollupPaymentsAreSentToBePaid(numberOfRollupPayments);
    }

    [Then(@"the amount of (.*) is sent to be paid for each payment in the curent Collection Month")]
    public void AmountIsSentToBePaidForEachPaymentInTheCurentCollectionMonth(decimal Amount)
    {
        var testData = _context.Get<TestData>();
        var finalisedPaymentsList = testData.FinalisedPaymentsList;
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
        var payments = _paymentsSqlClient.GetPayments(_context);
        var testData = _context.Get<TestData>();
        Assert.IsTrue(payments.Where(p => p.CollectionPeriod == testData.CurrentCollectionPeriod).All(p => p.SentForPayment));
    }

    [Then(@"all payments for the following collection periods are marked as not sent to payments BAU")]
    public void AllPaymentsForTheFollowingCollectionPeriodsAreAreMarkedAsNotSentToPaymentsBAU()
    {
        var currentAcademicYear = Convert.ToInt32(TableExtensions.CalculateAcademicYear("CurrentMonth+0"));

        var testData = _context.Get<TestData>();
        var payments = _paymentsSqlClient.GetPayments(_context);

        Assert.IsFalse(payments.Where(p => (p.CollectionYear > currentAcademicYear) ||
                                      (p.CollectionYear == currentAcademicYear && p.CollectionPeriod > testData.CurrentCollectionPeriod))
                                      .All(p => p.SentForPayment));
    }

    [Then(@"the unfunded payments that have already been sent to Payments BAU are not sent to be paid again")]
    public async Task ThenTheUnfundedPaymentsThatHaveAlreadyBeenSentToPaymentsBAUAreNotSentToBePaidAgain()
    {
        var testData = _context.Get<TestData>();

        await WaitHelper.WaitForUnexpected(() =>
        {
            var finalisedPaymentsList = FinalisedOnProgrammeLearningPaymentEventHandler.ReceivedEvents.Where(x => x.Message.ApprenticeshipKey == testData.LearningKey).Select(x=>x.Message).ToList();

            return finalisedPaymentsList.Count != 0;
        }, "Unexpected published Finalised On Programme Learning Payment events found");
    }
}
