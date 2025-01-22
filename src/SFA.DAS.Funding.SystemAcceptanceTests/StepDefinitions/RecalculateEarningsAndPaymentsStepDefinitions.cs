using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using System.Runtime.CompilerServices;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class RecalculateEarningsAndPaymentsStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly EarningsSqlClient _earningsEntitySqlClient;
        private readonly CalculateEarningsForLearningPaymentsStepDefinitions _calculateEarningsStepDefinitions;
        private readonly CalculateUnfundedPaymentsStepDefinitions _calculateUnfundedPaymentsStepDefinitions;
        private readonly PaymentsMessageHandler _paymentsMessageHelper;
        private readonly EarningsRecalculatedEventHelper _earningsRecalculatedEventHelper;
        private readonly PriceChangeApprovedEventHelper _priceChangeApprovedEventHelper;
        private readonly ApprenticeshipStartDateChangedEventHelper _apprenticeshipStartDateChangedEventHelper;
        private ApprenticeshipPriceChangedEvent _apprenticeshipPriceChangedEvent;
        private EarningsApprenticeshipModel? _earningsApprenticeshipModel;
        private DateTime _priceChangeEffectiveFrom;
        private DateTime _priceChangeApprovedDate;
        private decimal _newTrainingPrice;
        private decimal _newAssessmentPrice;
        private List<Payment> _paymentsEventList;
        private List<TestSupport.Payments> _paymentDbRecords;
        private readonly byte _currentCollectionPeriod;
        private readonly string _currentCollectionYear;
        private decimal _newEarningsAmount;
        private decimal _previousEarningsAmount;
        private decimal _fundingBandMax;
        private decimal _difference;
        private Guid _initialEarningsProfileId;
        private DateTime _startDateChangeApprovedDate;
        private DateTime _newStartDate;
        private DateTime _newEndDate;
        private DateTime? _plannedEndDate;
        private DateTime? _originalStartDate;



        public RecalculateEarningsAndPaymentsStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _earningsEntitySqlClient = new EarningsSqlClient();
            _paymentsMessageHelper = new PaymentsMessageHandler(context);
            _calculateEarningsStepDefinitions = new CalculateEarningsForLearningPaymentsStepDefinitions(_context);
            _calculateUnfundedPaymentsStepDefinitions = new CalculateUnfundedPaymentsStepDefinitions(context);
            _priceChangeApprovedEventHelper = new PriceChangeApprovedEventHelper(_context);
            _currentCollectionPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];
            _currentCollectionYear = TableExtensions.CalculateAcademicYear("0");
            _earningsRecalculatedEventHelper = new EarningsRecalculatedEventHelper(_context);
            _apprenticeshipStartDateChangedEventHelper = new ApprenticeshipStartDateChangedEventHelper(_context);

        }

        [Given(@"earnings have been calculated for an apprenticeship with (.*), (.*), (.*), and (.*)")]
        public async Task EarningsHaveBeenCalculatedForAnApprenticeshipWithAnd(TokenisableDateTime startDate, TokenisableDateTime plannedEndDate, decimal agreedPrice, string trainingCode)
        {
            _calculateEarningsStepDefinitions.ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(startDate, plannedEndDate, agreedPrice, trainingCode);
        }

        [Given(@"payments have been paid for an apprenticeship with (.*), (.*)")]
        public async Task GivenPaymentsHaveBeenCalculatedForAnApprenticeshipWithAnd(TokenisableDateTime startDate, TokenisableDateTime plannedEndDate)
        {
            await _calculateUnfundedPaymentsStepDefinitions.UnfundedPaymentsForTheRemainderOfTheApprenticeshipAreDetermined();

            _calculateUnfundedPaymentsStepDefinitions.UserWantsToProcessPaymentsForTheCurrentCollectionPeriod();

            await _calculateUnfundedPaymentsStepDefinitions.SchedulerTriggersUnfundedPaymentProcessing();

            var startDatePeriod = TableExtensions.Period[startDate.Value.ToString("MMMM")];

            _originalStartDate = startDate.Value;
            _plannedEndDate = plannedEndDate.Value;

            await _calculateUnfundedPaymentsStepDefinitions.UnpaidUnfundedPaymentsForTheCurrentCollectionMonthAndRollupPaymentsAreSentToBePaid(_currentCollectionPeriod - startDatePeriod);
        }


        [Given(@"the total price is above or below or at the funding band maximum")]
        public void TotalPriceIsBelowOrAtTheFundingBandMaximum()
        {
        }

        [Given(@"a price change request was sent on (.*)")]
        public void PriceChangeRequestWasSentOn(TokenisableDateTime effectiveFromDate)
        {
            _priceChangeEffectiveFrom = effectiveFromDate.Value;
        }

        [Given(@"the price change request has an approval date of (.*) with a new total (.*)")]
        public void PriceChangeRequestHasAnApprovalDateOfWithANewTotal(TokenisableDateTime approvedDate, decimal newTotalPrice)
        {
            _priceChangeApprovedDate = approvedDate.Value;
            _newTrainingPrice = newTotalPrice * 0.8m;
            _newAssessmentPrice = newTotalPrice * 0.2m;
        }

        [Given(@"a start date change request was sent with an approval date of (.*) with a new start date of (.*) and end date of (.*)")]
        public void StartDateChangeRequestWasSentWithAnApprovalDateAndNewStartDate(TokenisableDateTime approvedDate, TokenisableDateTime newStartDate, TokenisableDateTime newEndDate)
        {
            _startDateChangeApprovedDate = approvedDate.Value;
            _newStartDate = newStartDate.Value;
            _newEndDate = newEndDate.Value;
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

            _apprenticeshipPriceChangedEvent = _priceChangeApprovedEventHelper.CreatePriceChangeApprovedMessageWithCustomValues(_newTrainingPrice, _newAssessmentPrice, _priceChangeEffectiveFrom, _priceChangeApprovedDate);

            await _priceChangeApprovedEventHelper.PublishPriceChangeApprovedEvent(_apprenticeshipPriceChangedEvent);

            // Receive the update PaymentsGeneratedEvent
            await _paymentsMessageHelper.ReceivePaymentsEvent(_apprenticeshipPriceChangedEvent.ApprenticeshipKey);
        }

        [When(@"the start date change is approved")]
        public async Task StartDateChangeIsApproved()
        {
            // clear previous PaymentsGeneratedEvent before publishing StartDateChangeApproved Event 
            PaymentsGeneratedEventHandler.ReceivedEvents.Clear();

            var startDateChangedEvent = _apprenticeshipStartDateChangedEventHelper.CreateStartDateChangedMessageWithCustomValues(_newStartDate, _newEndDate, _startDateChangeApprovedDate);

            await _apprenticeshipStartDateChangedEventHelper.PublishApprenticeshipStartDateChangedEvent(startDateChangedEvent);

            // Receive the update PaymentsGeneratedEvent
            await _paymentsMessageHelper.ReceivePaymentsEvent(startDateChangedEvent.ApprenticeshipKey);
        }

        [Then(@"the earnings are recalculated based on the new instalment amount of (.*) from (.*) and (.*)")]
        public async Task EarningsAreRecalculatedBasedOnTheNewInstalmentAmountOfFromAnd(decimal newInstalmentAmount, int deliveryPeriod, string academicYearString)
        {
            var academicYear = TableExtensions.GetAcademicYear(academicYearString);

            await _earningsRecalculatedEventHelper.ReceiveEarningsRecalculatedEvent(_apprenticeshipPriceChangedEvent.ApprenticeshipKey);

            ApprenticeshipEarningsRecalculatedEvent recalculatedEarningsEvent = _context.Get<ApprenticeshipEarningsRecalculatedEvent>();

            recalculatedEarningsEvent.DeliveryPeriods.Where(Dp => Dp.AcademicYear >= Convert.ToInt16(academicYear) && Dp.Period >= deliveryPeriod).All(p => p.LearningAmount.Should().Equals(newInstalmentAmount));
        }

        [Then(@"the earnings are recalculated based on the new expected earnings (.*)")]
        public async Task EarningsAreRecalculatedBasedOnTheNewExpectedEarnings(decimal newInstalmentAmount)
        {
            await _earningsRecalculatedEventHelper.ReceiveEarningsRecalculatedEvent(_context.Get<EarningsGeneratedEvent>().ApprenticeshipKey);

            ApprenticeshipEarningsRecalculatedEvent recalculatedEarningsEvent = _context.Get<ApprenticeshipEarningsRecalculatedEvent>();

            recalculatedEarningsEvent.DeliveryPeriods.All(Dp => Dp.LearningAmount.Should().Equals(newInstalmentAmount));
        }



        [Then(@"for all the past census periods since (.*), where the payment has already been made, the amount is still same as previous earnings (.*) and are flagged as sent for payment")]
        public void ThenForAllThePastCensusPeriodsWhereThePaymentHasAlreadyBeenMadeTheAmountIsStillSameAsPreviousEarningsAndAreFlaggedAsSentForPayment(TokenisableDateTime startDate, double oldEarnings)
        {
            _paymentsEventList = _context.Get<PaymentsGeneratedEvent>().Payments;

            // validate PaymentsGenerateEvent

            var startDatePeriod = TableExtensions.Period[startDate.Value.ToString("MMMM")];
            _initialEarningsProfileId = _context.Get<Guid>("InitialEarningsProfileId");

            var expectedPaymentPeriods = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
                new Period(short.Parse(_currentCollectionYear), startDatePeriod),
                new Period(short.Parse(_currentCollectionYear), _currentCollectionPeriod),
                new PaymentExpectation
                {
                    Amount = (decimal)oldEarnings,
                    EarningsProfileId = _initialEarningsProfileId,
                    SentForPayment = true
                });

            expectedPaymentPeriods.AssertAgainstEventPayments(_paymentsEventList);

            // Validate Payments Entity

            var paymentsApiClient = new PaymentsSqlClient();

            _paymentDbRecords = paymentsApiClient.GetPaymentsModel(_context).Payments;

            _paymentDbRecords = _paymentDbRecords.Where(x => x.AcademicYear >= Convert.ToInt16(_currentCollectionYear)).ToList();

            expectedPaymentPeriods.AssertAgainstEntityArray(_paymentDbRecords);
        }

        [Then(@"the AgreedPrice on the earnings entity is updated to (.*)")]
        public void AgreedPriceOnTheEarningsEntityIsUpdated(decimal agreedPrice)
        {
            var apprenticeshipEntity = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

            Assert.AreEqual(agreedPrice, apprenticeshipEntity.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).Prices.MaxBy(x => x.StartDate).AgreedPrice);
        }

        [Then(@"the ActualStartDate (.*) and PlannedEndDate (.*) are updated on earnings entity")]
        public void ActualStartDateAndPlannedEndDateAreUpdatedOnEarningsEntity(TokenisableDateTime startDate, TokenisableDateTime endDate)
        {
            var apprenticeshipEntity = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

            Assert.IsNotNull(apprenticeshipEntity);
            Assert.AreEqual(startDate.Value, apprenticeshipEntity.Episodes.MinBy(x => x.Prices.MinBy(y => y.StartDate).StartDate).Prices.MinBy(x => x.StartDate).StartDate);
            Assert.AreEqual(endDate.Value, apprenticeshipEntity.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).Prices.MaxBy(x => x.StartDate)?.EndDate);
        }


        [Then(@"old earnings maintain their initial Profile Id and new earnings have a new profile id")]
        public void OldEarningsMaintainTheirInitialProfileId()
        {
            var earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

            _initialEarningsProfileId = _context.Get<Guid>("InitialEarningsProfileId");

            Assert.AreEqual(_initialEarningsProfileId, earningsApprenticeshipModel.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).EarningsProfileHistory.FirstOrDefault().EarningsProfileId, "Unexpected historical EarningsProfileId found");

            Assert.AreNotEqual(_initialEarningsProfileId, earningsApprenticeshipModel.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).EarningsProfile.EarningsProfileId, "Historical EarningsProfileId and new EarningsProfileId are the same");
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
            var startDatePeriod = TableExtensions.Period[earningsGeneratedEvent.StartDate.ToString("MMMM")];

            var expectedPaymentPeriods = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
                new Period(short.Parse(_currentCollectionYear), startDatePeriod),
                new Period(short.Parse(_currentCollectionYear), _currentCollectionPeriod),
                new PaymentExpectation
                {
                    Amount = difference,
                    SentForPayment = false
                });

            // Validate PaymentsGenerateEvent & Payments Entity
            expectedPaymentPeriods.AssertAgainstEventPayments(_paymentsEventList);

            expectedPaymentPeriods.AssertAgainstEntityArray(_paymentDbRecords);
        }

        [Then(@"for all the past census periods, new payments entries are created and marked as Not sent for payment with the difference between new earnings (.*) and old earnings")]
        public void NewPaymentsEntriesAreCreatedAndMarkedAsNotSentForPaymentInTheDurableEntityWithTheDifferenceBetweenNewEarningsAndOldEarnings(int expectedNewEarningAmount)
        {
            var earningsGeneratedEvent = _context.Get<EarningsGeneratedEvent>();

            _newEarningsAmount = expectedNewEarningAmount;

            _previousEarningsAmount = earningsGeneratedEvent.DeliveryPeriods[0].LearningAmount;

            var newStartDateCollectionPeriod = TableExtensions.Period[_newStartDate.ToString("MMMM")];
            var newStartDateCollectionYear = TableExtensions.CalculateAcademicYear("0", _newStartDate);

            //work out when the overlapping period starts where a diff payment needs to be created (will be difference depending on if the start date has moved backwards or forwards)
            var overlappingPeriodStart = _originalStartDate.GetValueOrDefault() < _newStartDate ? _newStartDate : _originalStartDate.GetValueOrDefault();

            var expectedPaymentPeriods = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
                new Period(overlappingPeriodStart),
                new Period(short.Parse(_currentCollectionYear), _currentCollectionPeriod),
                new PaymentExpectation
                {
                    Amount = _newEarningsAmount - _previousEarningsAmount,
                    SentForPayment = false
                });

            //Validate PaymentsGenerateEvent and payments entity

            expectedPaymentPeriods.AssertAgainstEventPayments(_paymentsEventList);

            expectedPaymentPeriods.AssertAgainstEntityArray(_paymentDbRecords);
        }

        [Then(@"for all payments for future collection periods are equal to the new earnings")]
        public void ThenForAllPaymentsForFutureCollectionPeriodsAreEqualToTheNewEarnings()
        {

            var firstDeliveryPeriod = _newStartDate.GetYearMonthFirstDay() > DateTime.Now.GetYearMonthFirstDay() ? new Period(_newStartDate) : new Period(DateTime.Now).GetNextPeriod();//latest of new start date vs current collection period

            var paymentPeriodExpectations = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
                firstDeliveryPeriod, 
                new Period(_plannedEndDate.Value).GetPreviousPeriod(),
                new PaymentExpectation
                {
                    Amount = _newEarningsAmount,
                    SentForPayment = false
                });

            //validate Payments Generated Event & Entity

            paymentPeriodExpectations.AssertAgainstEventPayments(_paymentsEventList);

            paymentPeriodExpectations.AssertAgainstEntityArray(_paymentDbRecords);
        }

        [Then(@"for all payments for past collection periods before the original start date \(new start date has moved backwards\) are equal to the new earnings")]
        public void ThenForAllPaymentsForPastCollectionPeriodsBeforeOriginalStartDateAreEqualToTheNewEarnings()
        {
            if (_newStartDate > _originalStartDate || _newStartDate > DateTime.Now) return; //no past periods this step applies to if new start date is not in the past and not before original start date

            var paymentPeriodExpectations = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
                new Period(_newStartDate),
                new Period(_originalStartDate.Value).GetPreviousPeriod(),
                new PaymentExpectation
                {
                    Amount = _newEarningsAmount,
                    SentForPayment = false
                });

            //validate Payments Generated Event  &Entity

            foreach (var periodExpectation in paymentPeriodExpectations)
            {
                Assert.That(_paymentsEventList.Any(x => x.AcademicYear == periodExpectation.DeliveryPeriod.AcademicYear && x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue && x.Amount == periodExpectation.Expectation.Amount),
                    $"Expected Amount for delivery period {periodExpectation.DeliveryPeriod.AcademicYear}-{periodExpectation.DeliveryPeriod.PeriodValue} to be {periodExpectation.Expectation.Amount} but was {_paymentsEventList.FirstOrDefault(x => x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue)?.Amount} " +
                    $" in Payments Generated Event post CoP - Future delivery periods");

                Assert.That(_paymentDbRecords.Any(x => x.AcademicYear == periodExpectation.DeliveryPeriod.AcademicYear && x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue && (decimal)x.Amount == periodExpectation.Expectation.Amount),
                    $"Expected Amount for delivery period {periodExpectation.DeliveryPeriod.AcademicYear}-{periodExpectation.DeliveryPeriod.PeriodValue} to be {periodExpectation.Expectation.Amount} but was {_paymentDbRecords.FirstOrDefault(x => x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue)?.Amount} " +
                    $" in Durable Entity - Future delivery periods");

                Assert.That(_paymentDbRecords.Any(x => x.AcademicYear == periodExpectation.DeliveryPeriod.AcademicYear && x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue && x.SentForPayment == periodExpectation.Expectation.SentForPayment),
                    $"Expected SentForPayment flag for delivery period {periodExpectation.DeliveryPeriod.AcademicYear}-{periodExpectation.DeliveryPeriod.PeriodValue} to be {periodExpectation.Expectation.SentForPayment} but was {_paymentDbRecords.FirstOrDefault(x => x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue)?.SentForPayment} " +
                    $" in Durable Entity - Future delivery periods");
            }
        }

        [Then(@"earnings prior to (.*) and (.*) are frozen with (.*)")]
        public void EarningsPriorToAndAreFrozenWith(int delivery_period, string academicYearString, double oldInstalmentAmount)
        {
            var academicYear = TableExtensions.GetAcademicYear(academicYearString);

            _earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

            var newEarningsProfile = _earningsApprenticeshipModel.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).EarningsProfile.Instalments;

            for (int i = 0; i < delivery_period - 1; i++)
            {
                if (newEarningsProfile[i].AcademicYear <= Convert.ToInt16(academicYear))
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
                _earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

                var historicalInstalments = _earningsApprenticeshipModel?.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate)?.StartDate)?.EarningsProfileHistory.FirstOrDefault()?.Instalments;

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

            var apprenticeshipPriceChangedEvent = fixture.Build<ApprenticeshipPriceChangedEvent>()
            .With(_ => _.ApprenticeshipKey, Guid.Parse(table.Rows[0]["apprenticeship_key"]))
            .With(_ => _.ApprenticeshipId, long.Parse(table.Rows[0]["apprenticeship_id"]))
            .With(_ => _.Episode, new ApprenticeshipEpisode
            {
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new ApprenticeshipEpisodePrice
                    {
                        EndPointAssessmentPrice = decimal.Parse(table.Rows[0]["assessment_price"]),
                        StartDate = _originalStartDate.GetValueOrDefault(),
                        EndDate = _plannedEndDate.GetValueOrDefault(),
                        TrainingPrice = decimal.Parse(table.Rows[0]["training_price"]),
                        FundingBandMaximum = (int)Math.Ceiling(_fundingBandMax),
                        Key = Guid.NewGuid()
                    }
                },
                EmployerAccountId = long.Parse(table.Rows[0]["employer_account_id"]),
                Ukprn = long.Parse(table.Rows[0]["provider_id"]),
                Key = _context.Get<Apprenticeships.Types.ApprenticeshipCreatedEvent>().Episode.Key
            })
            .With(_ => _.EffectiveFromDate, DateTime.Parse(table.Rows[0]["effective_from_date"]))
            .With(_ => _.ApprovedDate, DateTime.Parse(table.Rows[0]["approved_date"]))
            .Create();

            await TestServiceBus.Das.SendPriceChangeApprovedMessage(apprenticeshipPriceChangedEvent);
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