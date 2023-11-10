using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Payments.Messages.Core.Events;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class RecalculateEarningsAndPaymentsAfterApprovalOfPriceChangeRequestStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly CalculateEarningsForLearningPaymentsStepDefinitions _calculateEarningsStepDefinitions;
        private readonly CalculateUnfundedPaymentsStepDefinitions _calculateUnfundedPaymentsStepDefinitions;
        private readonly PaymentsMessageHandler _paymentsMessageHelper;
        private PriceChangeMessageHandler _priceChangeMessageHandler;
        private PriceChangeApprovedEvent _priceChangeApprovedEvent;
        private EarningsEntityModel _earningsApprenticeshipEntity;
        private DateTime _priceChangeEffectiveFrom;
        private DateTime _priceChangeApprovedDate;
        private decimal _newTrainingPrice;
        private decimal _newAssessmentPrice;
        private List<Payment> _paymentsEventList;
        private TestSupport.Payments[] _paymentsEntityArray;
        private readonly byte _currentCollectionPeriod;

        public RecalculateEarningsAndPaymentsAfterApprovalOfPriceChangeRequestStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _paymentsMessageHelper = new PaymentsMessageHandler(context);
            _calculateEarningsStepDefinitions = new CalculateEarningsForLearningPaymentsStepDefinitions(_context);
            _calculateUnfundedPaymentsStepDefinitions = new CalculateUnfundedPaymentsStepDefinitions(context);
            _priceChangeMessageHandler = new PriceChangeMessageHandler(_context);
            _currentCollectionPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];
        }

        [Given(@"earnings have been calculated for an apprenticeship with (.*), (.*), (.*), and (.*)")]
        public async Task EarningsHaveBeenCalculatedForAnApprenticeshipWithAnd(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode)
        {
            _calculateEarningsStepDefinitions.ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(startDate, plannedEndDate, agreedPrice, trainingCode);

            await _calculateEarningsStepDefinitions.TheApprenticeshipCommitmentIsApproved();
        }

        [Given(@"payments have been paid for an apprenticeship with (.*), (.*), (.*), and (.*)")]
        public async Task GivenPaymentsHaveBeenCalculatedForAnApprenticeshipWithAnd(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice, string  trainingCode)
        {
            _calculateEarningsStepDefinitions.ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(startDate, plannedEndDate, agreedPrice, trainingCode);

            await _calculateEarningsStepDefinitions.TheApprenticeshipCommitmentIsApproved();

            await _calculateUnfundedPaymentsStepDefinitions.UnfundedPaymentsForTheRemainderOfTheApprenticeshipAreDetermined();

            _calculateUnfundedPaymentsStepDefinitions.UserWantsToProcessPaymentsForTheCurrentCollectionPeriod();

            await _calculateUnfundedPaymentsStepDefinitions.SchedulerTriggersUnfundedPaymentProcessing();

            await _calculateUnfundedPaymentsStepDefinitions.UnpaidUnfundedPaymentsForTheCurrentCollectionMonthAndRollupPaymentsAreSentToBePaid(_currentCollectionPeriod-1);
        }


        [Given(@"the total price is above or below or at the funding band maximum")]
        public void TotalPriceIsBelowOrAtTheFundingBandMaximum()
        {
        }

        [Given(@"a price change request was sent on (.*)")]
        public void PriceChangeRequestWasSentOn(DateTime effectiveFromDate)
        {
            _priceChangeEffectiveFrom = effectiveFromDate;
        }

        [Given(@"the price change request has an approval date of (.*) with a new total (.*)")]
        public void PriceChangeRequestHasAnApprovalDateOfWithANewTotal(DateTime approvedDate, decimal newTotalPrice)
        {
            _priceChangeApprovedDate = approvedDate;
            _newTrainingPrice = newTotalPrice * 0.8m;
            _newAssessmentPrice = newTotalPrice * 0.2m;
        }

        [When(@"the price change is approved")]
        public async Task PriceChangeIsApproved()
        {
            _priceChangeApprovedEvent = _priceChangeMessageHandler.CreatePriceChangeApprovedMessageWithCustomValues(_newTrainingPrice, _newAssessmentPrice, _priceChangeEffectiveFrom, _priceChangeApprovedDate);

            await _priceChangeMessageHandler.PublishPriceChangeApprovedEvent(_priceChangeApprovedEvent);
        }

        [Then(@"the earnings are recalculated based on the new instalment amount of (.*) from (.*) and (.*)")]
        public async Task EarningsAreRecalculatedBasedOnTheNewInstalmentAmountOfFromAnd(decimal newInstalmentAmount, int deliveryPeriod, int academicYear)
        {
            await _priceChangeMessageHandler.ReceiveEarningsRecalculatedEvent(_priceChangeApprovedEvent.ApprenticeshipKey);

            ApprenticeshipEarningsRecalculatedEvent recalculatedEarningsEvent = _context.Get<ApprenticeshipEarningsRecalculatedEvent>();

            recalculatedEarningsEvent.DeliveryPeriods.Where(Dp => Dp.AcademicYear >= academicYear && Dp.Period >= deliveryPeriod).All(p => p.LearningAmount.Should().Equals(newInstalmentAmount));
        }

        [Then(@"for all the past census periods, where the payment has already been made, the amount is still same as previous earnings (.*) and are flagged as sent for payment")]
        public async Task ThenForAllThePastCensusPeriodsWhereThePaymentHasAlreadyBeenMadeTheAmountIsStillSameAsPreviousEarningsAndAreFlaggedAsSentForPayment(double oldEarnings)
        {
            // clear this before you publish the RecalculatedPayment Event 
            PaymentsGeneratedEventHandler.ReceivedEvents.Clear();

            await _paymentsMessageHelper.ReceivePaymentsEvent(_priceChangeApprovedEvent.ApprenticeshipKey);

            _paymentsEventList = _context.Get<PaymentsGeneratedEvent>().Payments;

            // validate PaymentsGenerateEvent

            for (int i = 0; i < _currentCollectionPeriod; i++)
            {
                Assert.AreEqual(oldEarnings, _paymentsEventList[i].Amount, $"Expected Amount for delivery period {i + 1} to be {oldEarnings} but was {_paymentsEventList[i].Amount} " +
                    $" in Payments Generated Event post CoP - Payments already made");
            }

            // Validate Payments Entity

            var paymentsApiClient = new PaymentsEntityApiClient(_context);

            _paymentsEntityArray = paymentsApiClient.GetPaymentsEntityModel().Result.Model.Payments;

            for (int i = 0; i < _currentCollectionPeriod; i++)
            {
                Assert.AreEqual(oldEarnings, _paymentsEntityArray[i].Amount, $"Expected Amount to be {oldEarnings} for payment record {i + 1} but was {_paymentsEntityArray[i + 1].Amount} in Durable Entity");
                Assert.IsTrue(_paymentsEntityArray[i].SentForPayment, $"Expected SentForPayment flag to be True for payment record {i + 1} in durable entity");
            }
        }

        [Then(@"for all the past census periods, new payments entries are created and marked as Not sent for payment in the durable entity with the difference (.*) between new and old earnings")]
        public void NewPaymentsEntriesAreCreatedAndMarkedAsNotSentForPaymentInTheDurableEntityWithTheDifferenceBetweenNewAndOldEarnings(double difference)
        {

            // Validate PaymentsGenerateEvent

            for (int i = _currentCollectionPeriod; i < _currentCollectionPeriod * 2; i++)
            {
                Assert.AreEqual(difference, _paymentsEventList[i].Amount, $"Expected Amount for delivery period {i + 1} to be {difference} but was {_paymentsEventList[i].Amount} " +
                    $" in Payments Generated Event post CoP - Different between new and old payments");
            }

            // Validate Payments Entity

            for (int i = _currentCollectionPeriod; i < _currentCollectionPeriod*2; i++)
            {
                Assert.AreEqual(difference, _paymentsEntityArray[i].Amount, $"Expected Amount to be {difference} for payment record {i + 1} but was {_paymentsEntityArray[i+1].Amount} in Durable Entity");
                Assert.IsFalse(_paymentsEntityArray[i].SentForPayment, $"Expected SentForPayment flag to be False for payment record {i + 1} in durable entity");
            }
        }

        [Then(@"for all payments for future collection periods are equal to the new earnings (.*)")]
        public void ThenForAllPaymentsForFutureCollectionPeriodsAreEqualToTheNewEarnings(double newEarnings)
        {
            // validate Payments Generated Event 
            for (int i = _currentCollectionPeriod * 2; i < _paymentsEventList.Count; i++)
            {
                Assert.AreEqual(newEarnings, _paymentsEventList[i].Amount, $"Expected Amount for delivery period {i + 1} to be {newEarnings} but was {_paymentsEventList[i].Amount} " +
                    $" in Payments Generated Event post CoP - Future delivery periods");
            }

            // validate Payments Entity

            for (int i = _currentCollectionPeriod * 2; i < _paymentsEntityArray.Length; i++)
            {
                Assert.AreEqual(newEarnings, _paymentsEntityArray[i].Amount, $"Expected Amount to be {newEarnings} for payment record {i + 1} but was {_paymentsEntityArray[i + 1].Amount} in Durable Entity");
                Assert.IsFalse(_paymentsEntityArray[i].SentForPayment, $"Expected SentForPayment flag to be False for payment record {i + 1} in durable entity");
            }
        }

        [Then(@"earnings prior to (.*) and (.*) are frozen with (.*)")]
        public void EarningsPriorToAndAreFrozenWith(int delivery_period, int academicYear, double oldInstalmentAmount)
        {
            var earningsApiClient = new EarningsEntityApiClient(_context);

            _earningsApprenticeshipEntity = earningsApiClient.GetEarningsEntityModel();

            var newEarningsProfile = _earningsApprenticeshipEntity.Model.EarningsProfile.Instalments;

            for (int i = 0; i < delivery_period - 1; i++)
            {
                if (newEarningsProfile[i].AcademicYear <= academicYear)
                {
                    Assert.AreEqual(oldInstalmentAmount, newEarningsProfile[i].Amount, $"Earning prior to DeliveryPeriod {delivery_period} " +
                        $" are not frozen. Expected Amount for Delivery Period: {newEarningsProfile[i].DeliveryPeriod} and AcademicYear: " +
                        $" {newEarningsProfile[i].AcademicYear} to be {oldInstalmentAmount} but was {newEarningsProfile[i].Amount}");
                }
            }
        }

        [Then(@"the history of old earnings is maintained with (.*)")]
        public async Task HistoryOfOldEarningsIsMaintained(double old_instalment_amount)
        {
            await WaitHelper.WaitForIt(() =>
            {
                var historicalInstalments = _earningsApprenticeshipEntity.Model.EarningsProfileHistory[0].Record.Instalments;

                if (historicalInstalments != null)
                {
                    foreach (var instalment in historicalInstalments)
                    {
                        Assert.AreEqual(old_instalment_amount, instalment.Amount, $"Expected historical earnings amount to be {old_instalment_amount}, but was {instalment.Amount}");
                    }
                    return true;
                }
                return false;
            }, "Failed to find installments in Earnings Profile History");
        }
    }
}