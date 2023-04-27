using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using ApprenticeshipCreatedEvent = SFA.DAS.Apprenticeships.Types.ApprenticeshipCreatedEvent;
using System.Linq;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class CalculateUnfundedPaymentsSetpDefinitions
    {
        private readonly ScenarioContext _context;
        private List<Payment> _payments;
        private readonly PaymentsMessageHandler _paymentsMessageHelper;
        private ReleasePaymentsCommand _releasePaymentsCommand;


        public CalculateUnfundedPaymentsSetpDefinitions(ScenarioContext context)
        {
            _context = context;
            _paymentsMessageHelper = new PaymentsMessageHandler(context);
        }

        [When(@"the Unfunded Payments for the remainder of the apprenticeship are determined")]
        public async Task UnfundedPaymentsForTheRemainderOfTheApprenticeshipAreDetermined()
        {
            await _paymentsMessageHelper.ReceivePaymentsEvent(_context.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey);

            _payments = _context.Get<PaymentsGeneratedEvent>().Payments;
        }

        [Then(@"the Unfunded Payments for every earning is created in the following month")]
        public void UnfundedPaymentsForEveryEarningIsCreatedInTheFollowingMonth(Table table) => _payments.ShouldHaveCorrectPaymentsGenerated(table.ToExpectedPayments());

        [Then(@"Unfunded Payments for the appreticeship including rollup payments are calculated as below")]
        public void UnfundedPaymentsForTheAppreticeshipIncludingRollupPaymentsAreCalculated(Table table) => _payments.ShouldHaveCorrectPaymentsGenerated(table.ToExpectedRollupPayments());

        [Then(@"the newly calculated Unfunded Payments are marked as not sent to payments BAU")]
        public void NewlyCalculatedUnfundedPaymentsAreMarkedAsNotSentToPaymentsBAU()
        {
            var apiClient = new PaymentsEntityApiClient(_context);

            var payments = apiClient.GetPaymentsEntityModel().Model.Payments;

            Assert.IsTrue(payments.All(x => x.SentForPayment == false)); 
        }

        [Given(@"the user wants to process payments for the current collection Period")]
        public void UserWantsToProcessPaymentsForTheCurrentCollectionPeriod()
        {
            _releasePaymentsCommand = new ReleasePaymentsCommand();
            _releasePaymentsCommand.CollectionMonth = TableExtensions.Period[DateTime.Now.ToString("MMMM")];
        }

        [When(@"the scheduler triggers Unfunded Payment processing")]
        public async Task SchedulerTriggersUnfundedPaymentProcessing()
        {
            await _paymentsMessageHelper.PublishReleasePaymentsCommand(_releasePaymentsCommand);
        }

        [Then(@"the unpaid unfunded payments for the specified Collection Month are sent to be paid")]
        public async Task UnpaidUnfundedPaymentsForTheSpecifiedCollectionMonthAreSentToBePaid()
        {
            await WaitHelper.WaitForIt(() =>
            {
                var finalisedPaymentEvent =
                    FinalisedOnProgrammeLearningPaymentEventHandler.ReceivedEvents.ToList<FinalisedOnProgammeLearningPaymentEvent>();

                finalisedPaymentEvent.Where(x => x.ApprenticeshipKey == _context.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey);

                if (finalisedPaymentEvent.Count == 0) return false;

                return finalisedPaymentEvent.All(x => x.CollectionMonth == TableExtensions.Period[DateTime.Now.ToString("MMMM")]);
            }, "Failed to find published Finalised On Programme Learning Payment event");
        }

    }
}
