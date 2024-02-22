using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using PriceChangeApprovedEvent = SFA.DAS.Apprenticeships.Types.PriceChangeApprovedEvent;

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
        private readonly string _currentCollectionYear;
        private decimal _newEarningsAmount;
        private decimal _fundingBandMax;



        public RecalculateEarningsAndPaymentsAfterApprovalOfPriceChangeRequestStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _paymentsMessageHelper = new PaymentsMessageHandler(context);
            _calculateEarningsStepDefinitions = new CalculateEarningsForLearningPaymentsStepDefinitions(_context);
            _calculateUnfundedPaymentsStepDefinitions = new CalculateUnfundedPaymentsStepDefinitions(context);
            _priceChangeMessageHandler = new PriceChangeMessageHandler(_context);
            _currentCollectionPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];
            _currentCollectionYear = TableExtensions.CalculateAcademicYear("0");
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

        [Given(@"funding band max (.*) is determined for the training code")]
        public void GivenFundingBandMaxIsDeterminedForTheTrainingCode(decimal fundingBandMax)
        {
            _fundingBandMax = fundingBandMax;
        }


        [When(@"the price change is approved")]
        public async Task PriceChangeIsApproved()
        {
            // clear previous PaymentsGeneratedEvent before publishing PriceChangeApproved Event 
            PaymentsGeneratedEventHandler.ReceivedEvents.Clear();

            _priceChangeApprovedEvent = _priceChangeMessageHandler.CreatePriceChangeApprovedMessageWithCustomValues(_newTrainingPrice, _newAssessmentPrice, _priceChangeEffectiveFrom, _priceChangeApprovedDate);

            await _priceChangeMessageHandler.PublishPriceChangeApprovedEvent(_priceChangeApprovedEvent);

            // Receive the update PaymentsGeneratedEvent
            await _paymentsMessageHelper.ReceivePaymentsEvent(_priceChangeApprovedEvent.ApprenticeshipKey);
        }

        [Then(@"the earnings are recalculated based on the new instalment amount of (.*) from (.*) and (.*)")]
        public async Task EarningsAreRecalculatedBasedOnTheNewInstalmentAmountOfFromAnd(decimal newInstalmentAmount, int deliveryPeriod, int academicYear)
        {
            await _priceChangeMessageHandler.ReceiveEarningsRecalculatedEvent(_priceChangeApprovedEvent.ApprenticeshipKey);

            ApprenticeshipEarningsRecalculatedEvent recalculatedEarningsEvent = _context.Get<ApprenticeshipEarningsRecalculatedEvent>();

            recalculatedEarningsEvent.DeliveryPeriods.Where(Dp => Dp.AcademicYear >= academicYear && Dp.Period >= deliveryPeriod).All(p => p.LearningAmount.Should().Equals(newInstalmentAmount));
        }

        [Then(@"for all the past census periods, where the payment has already been made, the amount is still same as previous earnings (.*) and are flagged as sent for payment")]
        public void ThenForAllThePastCensusPeriodsWhereThePaymentHasAlreadyBeenMadeTheAmountIsStillSameAsPreviousEarningsAndAreFlaggedAsSentForPayment(double oldEarnings)
        {
            _paymentsEventList = _context.Get<PaymentsGeneratedEvent>().Payments;

            // validate PaymentsGenerateEvent - remove payments from previous academic years 

            _paymentsEventList = _paymentsEventList.Where(x => x.AcademicYear >= Convert.ToInt16(_currentCollectionYear)).ToList();

            for (int i = 0; i < _currentCollectionPeriod; i++)
            {
                Assert.AreEqual(oldEarnings, _paymentsEventList[i].Amount, $"Expected Amount for delivery period {i + 1} to be {oldEarnings} but was {_paymentsEventList[i].Amount} " +
                    $" in Payments Generated Event post CoP - Payments already made");
            }

            // Validate Payments Entity - remove payments from previous academic years

            var paymentsApiClient = new PaymentsEntityApiClient(_context);

            _paymentsEntityArray = paymentsApiClient.GetPaymentsEntityModel().Model.Payments;

            _paymentsEntityArray = _paymentsEntityArray.Where(x => x.AcademicYear >= Convert.ToInt16(_currentCollectionYear)).ToArray();

            for (int i = 0; i < _currentCollectionPeriod; i++)
            {
                Assert.AreEqual(oldEarnings, _paymentsEntityArray[i].Amount, $"Expected Amount to be {oldEarnings} for payment record {i + 1} but was {_paymentsEntityArray[i].Amount} in Durable Entity");
                Assert.IsTrue(_paymentsEntityArray[i].SentForPayment, $"Expected SentForPayment flag to be True for payment record {i + 1} in durable entity");
            }
        }

        [Then(@"for all the past census periods, new payments entries are created and marked as Not sent for payment with the difference between new and old earnings")]
        public void NewPaymentsEntriesAreCreatedAndMarkedAsNotSentForPaymentInTheDurableEntityWithTheDifferenceBetweenNewAndOldEarnings()
        {
            var earningsGeneratedEvent = _context.Get<EarningsGeneratedEvent>();

            var proposedNewTotalPrice = _newTrainingPrice + _newAssessmentPrice;

            var apprenticeshipDurationInMonth = CalculateMonthsDifference(earningsGeneratedEvent.PlannedEndDate, earningsGeneratedEvent.StartDate);

            _newEarningsAmount = CalculateNewEarnings(proposedNewTotalPrice, _fundingBandMax, apprenticeshipDurationInMonth, earningsGeneratedEvent.DeliveryPeriods[0].LearningAmount,
                earningsGeneratedEvent.StartDate, _priceChangeEffectiveFrom);

            var difference = _newEarningsAmount - earningsGeneratedEvent.DeliveryPeriods[0].LearningAmount;

            // Validate PaymentsGenerateEvent

            for (int i = _currentCollectionPeriod; i < _currentCollectionPeriod * 2; i++)
            {
                Assert.AreEqual(difference, _paymentsEventList[i].Amount, $"Expected Amount for delivery period {_paymentsEventList[i].DeliveryPeriod} to be {difference} but was {_paymentsEventList[i].Amount} " +
                    $" in Payments Generated Event post CoP - Different between new and old payments");
            }

            // Validate Payments Entity

            for (int i = _currentCollectionPeriod; i < _currentCollectionPeriod*2; i++)
            {
                Assert.AreEqual(difference, _paymentsEntityArray[i].Amount, $"Expected Amount to be {difference} for payment record {i + 1} but was {_paymentsEntityArray[i].Amount} in Durable Entity");
                Assert.IsFalse(_paymentsEntityArray[i].SentForPayment, $"Expected SentForPayment flag to be False for payment record {i + 1} in durable entity");
            }
        }

        [Then(@"for all payments for future collection periods are equal to the new earnings")]
        public void ThenForAllPaymentsForFutureCollectionPeriodsAreEqualToTheNewEarnings()
        {
            // validate Payments Generated Event 
            for (int i = _currentCollectionPeriod * 2; i < _paymentsEventList.Count; i++)
            {
                Assert.AreEqual(_newEarningsAmount, _paymentsEventList[i].Amount, $"Expected Amount for delivery period {_paymentsEventList[i].DeliveryPeriod} to be {_newEarningsAmount} but was {_paymentsEventList[i].Amount} " +
                    $" in Payments Generated Event post CoP - Future delivery periods");
            }

            // validate Payments Entity

            for (int i = _currentCollectionPeriod * 2; i < _paymentsEntityArray.Length; i++)
            {
                Assert.AreEqual(_newEarningsAmount, _paymentsEntityArray[i].Amount, $"Expected Amount to be {_newEarningsAmount} for payment record {i + 1} but was {_paymentsEntityArray[i].Amount} in Durable Entity");
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

        [Given(@"a price change event is approved")]
        public async Task GivenAPriceChangeEventIsApproved(Table table)
        {
            var fixture = new Fixture();
             
            var priceChangeApprovedEvent = fixture.Build<PriceChangeApprovedEvent>()
            .With(_ => _.ApprenticeshipKey, Guid.Parse(table.Rows[0]["apprenticeship_key"]))
            .With(_ => _.ApprenticeshipId, long.Parse(table.Rows[0]["apprenticeship_id"]))
            .With(_ => _.TrainingPrice, decimal.Parse(table.Rows[0]["training_price"]))
            .With(_ => _.AssessmentPrice, decimal.Parse(table.Rows[0]["assessment_price"]))
            .With(_ => _.EffectiveFromDate, DateTime.Parse(table.Rows[0]["effective_from_date"]))
            .With(_ => _.ApprovedDate, DateTime.Parse(table.Rows[0]["approved_date"]))
            .With(_ => _.EmployerAccountId, long.Parse(table.Rows[0]["employer_account_id"]))
            .With(_ => _.ProviderId, long.Parse(table.Rows[0]["provider_id"]))
            .Create();

            await TestServiceBus.Das.SendPriceChangeApprovedMessage(priceChangeApprovedEvent);
        }


        static int CalculateMonthsDifference(DateTime endDate, DateTime startDate)
        {
            int monthsDifference = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

            if (endDate.Day < startDate.Day) monthsDifference--;

            return monthsDifference;
        }

        static decimal CalculateNewEarnings(decimal proposedNewTotalPrice, decimal fundingBandMax, int apprenticeshipDurationInMonth, decimal oldEarnings, DateTime startDate, DateTime copStartDate)
        {
            var priceToUse = proposedNewTotalPrice > fundingBandMax ? fundingBandMax : proposedNewTotalPrice;

            var censusDatesPassedUpToCoPEffectiveFromDate = CalculateMonthsDifference(copStartDate, startDate);

            var amountAlreadyPaid = censusDatesPassedUpToCoPEffectiveFromDate * oldEarnings;

            return ((priceToUse * 0.8m) - amountAlreadyPaid) / (apprenticeshipDurationInMonth - censusDatesPassedUpToCoPEffectiveFromDate);
        }
    }
}