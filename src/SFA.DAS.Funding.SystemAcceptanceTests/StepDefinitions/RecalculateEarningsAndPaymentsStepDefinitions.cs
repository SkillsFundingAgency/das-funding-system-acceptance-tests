using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class RecalculateEarningsAndPaymentsStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly PaymentsFunctionsClient _paymentsFunctionsClient;
    private readonly EarningsSqlClient _earningsEntitySqlClient;
    private readonly PaymentsSqlClient _paymentsSqlClient;
    private readonly ApprenticeshipsInnerApiHelper _apprenticeshipsInnerApiHelper;

    public RecalculateEarningsAndPaymentsStepDefinitions(
        ScenarioContext context, 
        PaymentsFunctionsClient paymentsFunctionsClient, 
        PaymentsSqlClient paymentsSqlClient,
        EarningsSqlClient earningsSqlClient,
        ApprenticeshipsInnerApiHelper apprenticeshipsInnerApiHelper)
    {
        _context = context;
        _earningsEntitySqlClient = earningsSqlClient;
        _paymentsSqlClient = paymentsSqlClient;
        _apprenticeshipsInnerApiHelper = apprenticeshipsInnerApiHelper;
        _paymentsFunctionsClient = paymentsFunctionsClient;
    }

    [Given(@"payments have been paid for an apprenticeship with (.*), (.*)")]
    public async Task GivenPaymentsHaveBeenCalculatedForAnApprenticeshipWithAnd(TokenisableDateTime startDate, TokenisableDateTime plannedEndDate)
    {
        var testData = _context.Get<TestData>();

        await _paymentsFunctionsClient.InvokeReleasePaymentsHttpTrigger(testData.CurrentCollectionPeriod,
            Convert.ToInt16(testData.CurrentCollectionYear));

        var startDatePeriod = TableExtensions.Period[startDate.Value.ToString("MMMM")];
        var startDateYear = TableExtensions.CalculateAcademicYear("0", startDate.Value);

        testData.OriginalStartDate = startDate.Value;
        testData.PlannedEndDate = plannedEndDate.Value;

        var firstDeliveryPeriod = short.Parse(startDateYear) < short.Parse(testData.CurrentCollectionYear) ? TableExtensions.Period["August"] : startDatePeriod;


        await _context.UnpaidUnfundedPaymentsForTheCurrentCollectionMonthAndRollupPaymentsAreSentToBePaid(testData.CurrentCollectionPeriod - firstDeliveryPeriod);

    }


    [Given(@"the total price is above or below or at the funding band maximum")]
    public void TotalPriceIsBelowOrAtTheFundingBandMaximum()
    {
    }

    [Given(@"a price change request was sent on (.*)")]
    public void PriceChangeRequestWasSentOn(TokenisableDateTime effectiveFromDate)
    {
        var testData = _context.Get<TestData>();
        testData.PriceChangeEffectiveFrom = effectiveFromDate.Value;
    }

    [Given(@"the price change request has an approval date of (.*) with a new total (.*)")]
    public void PriceChangeRequestHasAnApprovalDateOfWithANewTotal(TokenisableDateTime approvedDate, decimal newTotalPrice)
    {
        var testData = _context.Get<TestData>();
        testData.PriceChangeApprovedDate = approvedDate.Value;
        testData.NewTrainingPrice = newTotalPrice * 0.8m;
        testData.NewAssessmentPrice = newTotalPrice * 0.2m;
    }

    [Given(@"a start date change request was sent with an approval date of (.*) with a new start date of (.*) and end date of (.*)")]
    public void StartDateChangeRequestWasSentWithAnApprovalDateAndNewStartDate(TokenisableDateTime approvedDate, TokenisableDateTime newStartDate, TokenisableDateTime newEndDate)
    {
        var testData = _context.Get<TestData>();
        testData.StartDateChangeApprovedDate = approvedDate.Value;
        testData.NewStartDate = newStartDate.Value;
        testData.NewEndDate = newEndDate.Value;
    }

    [Given(@"funding band max (.*) is determined for the training code")]
    public void GivenFundingBandMaxIsDeterminedForTheTrainingCode(decimal fundingBandMax)
    {
        var testData = _context.Get<TestData>();
        testData.FundingBandMax = fundingBandMax;
    }


    [When(@"the price change is approved")]
    public async Task PriceChangeIsApproved()
    {
        var testData = _context.Get<TestData>();
        // clear previous PaymentsGeneratedEvent before publishing PriceChangeApproved Event 
        PaymentsGeneratedEventHandler.Clear(x => x.ApprenticeshipKey == testData.ApprenticeshipKey);

        await _apprenticeshipsInnerApiHelper.CreatePriceChangeRequest(_context, testData.NewTrainingPrice, testData.NewAssessmentPrice, testData.PriceChangeEffectiveFrom);
        await _apprenticeshipsInnerApiHelper.ApprovePendingPriceChangeRequest(_context, testData.NewTrainingPrice, testData.NewAssessmentPrice, testData.PriceChangeApprovedDate);
    }

    [When(@"the start date change is approved")]
    public async Task StartDateChangeIsApproved()
    {
        var testData = _context.Get<TestData>();
        // clear previous PaymentsGeneratedEvent before publishing StartDateChangeApproved Event 
        PaymentsGeneratedEventHandler.Clear(x => x.ApprenticeshipKey == testData.ApprenticeshipKey);

        var startDateChangedEvent = ApprenticeshipStartDateChangedEventHelper.CreateStartDateChangedMessageWithCustomValues(_context, testData.NewStartDate, testData.NewEndDate, testData.StartDateChangeApprovedDate);

        await ApprenticeshipStartDateChangedEventHelper.PublishApprenticeshipStartDateChangedEvent(startDateChangedEvent);
    }

    [Then(@"the earnings are recalculated based on the new instalment amount of (.*) from (.*) and (.*)")]
    public async Task EarningsAreRecalculatedBasedOnTheNewInstalmentAmountOfFromAnd(decimal newInstalmentAmount, int deliveryPeriod, string academicYearString)
    {
        var testData = _context.Get<TestData>();
        var academicYear = TableExtensions.GetAcademicYear(academicYearString);

        await _context.ReceiveEarningsRecalculatedEvent(testData.ApprenticeshipCreatedEvent.ApprenticeshipKey);

        testData.ApprenticeshipEarningsRecalculatedEvent!.DeliveryPeriods.Where(Dp => Dp.AcademicYear >= Convert.ToInt16(academicYear) && Dp.Period >= deliveryPeriod).All(p => p.LearningAmount.Should().Equals(newInstalmentAmount));
    }

    [Then(@"the earnings are recalculated based on the new expected earnings (.*)")]
    public async Task EarningsAreRecalculatedBasedOnTheNewExpectedEarnings(decimal newInstalmentAmount)
    {
        var testData = _context.Get<TestData>();

        await _context.ReceiveEarningsRecalculatedEvent(testData.ApprenticeshipKey);

        testData.ApprenticeshipEarningsRecalculatedEvent.DeliveryPeriods.All(Dp => Dp.LearningAmount.Should().Equals(newInstalmentAmount));
    }

    [Then(@"for all the past census periods since (.*), where the payment has already been made, the amount is still same as previous earnings (.*) and are flagged as sent for payment")]
    public void ThenForAllThePastCensusPeriodsWhereThePaymentHasAlreadyBeenMadeTheAmountIsStillSameAsPreviousEarningsAndAreFlaggedAsSentForPayment(TokenisableDateTime startDate, double oldEarnings)
    {
        var testData = _context.Get<TestData>();

        // validate PaymentsGenerateEvent
        var startDatePeriod = TableExtensions.Period[startDate.Value.ToString("MMMM")];

        var expectedPaymentPeriods = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
            new Period(short.Parse(testData.CurrentCollectionYear), startDatePeriod),
            new Period(short.Parse(testData.CurrentCollectionYear), testData.CurrentCollectionPeriod),
            new PaymentExpectation
            {
                Amount = (decimal)oldEarnings,
                EarningsProfileId = testData.InitialEarningsProfileId,
                SentForPayment = true
            });

        expectedPaymentPeriods.AssertAgainstEventPayments(testData.PaymentsGeneratedEvent.Payments);

        // Validate Payments Entity

        var paymentDbRecords = _paymentsSqlClient.GetPaymentsModel(_context).Payments;

        testData.PaymentDbRecords = paymentDbRecords.Where(x => x.AcademicYear >= Convert.ToInt16(testData.CurrentCollectionYear)).ToList();

        expectedPaymentPeriods.AssertAgainstEntityArray(testData.PaymentDbRecords);
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
        var testData = _context.Get<TestData>();
        var earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

        Assert.AreEqual(testData.InitialEarningsProfileId, earningsApprenticeshipModel.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).EarningsProfileHistory.FirstOrDefault().EarningsProfileId, "Unexpected historical EarningsProfileId found");
        Assert.AreNotEqual(testData.InitialEarningsProfileId, earningsApprenticeshipModel.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).EarningsProfile.EarningsProfileId, "Historical EarningsProfileId and new EarningsProfileId are the same");
    }

    [Then(@"for all the past census periods, new payments entries are created and marked as Not sent for payment with the difference between new and old earnings")]
    public void NewPaymentsEntriesAreCreatedAndMarkedAsNotSentForPaymentInTheDurableEntityWithTheDifferenceBetweenNewAndOldEarnings()
    {
        var testData = _context.Get<TestData>();
        var earningsGeneratedEvent = testData.EarningsGeneratedEvent;

        var proposedNewTotalPrice = testData.NewTrainingPrice + testData.NewAssessmentPrice;

        var apprenticeshipDurationInMonth = CalculateMonthsDifference(earningsGeneratedEvent.PlannedEndDate, earningsGeneratedEvent.StartDate);

        testData.NewEarningsAmount = CalculateNewEarnings(proposedNewTotalPrice, testData.FundingBandMax, apprenticeshipDurationInMonth, earningsGeneratedEvent.DeliveryPeriods[0].LearningAmount,
            earningsGeneratedEvent.StartDate, testData.PriceChangeEffectiveFrom);

        var difference = testData.NewEarningsAmount - earningsGeneratedEvent.DeliveryPeriods[0].LearningAmount;
        var startDatePeriod = TableExtensions.Period[earningsGeneratedEvent.StartDate.ToString("MMMM")];

        var expectedPaymentPeriods = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
            new Period(short.Parse(testData.CurrentCollectionYear), startDatePeriod),
            new Period(short.Parse(testData.CurrentCollectionYear), testData.CurrentCollectionPeriod),
            new PaymentExpectation
            {
                Amount = difference,
                SentForPayment = false
            });

        // Validate PaymentsGenerateEvent & Payments Entity
        expectedPaymentPeriods.AssertAgainstEventPayments(testData.PaymentsGeneratedEvent.Payments);

        expectedPaymentPeriods.AssertAgainstEntityArray(testData.PaymentDbRecords);
    }

    [Then(@"for all the past census periods, new payments entries are created and marked as Not sent for payment with the difference between new earnings (.*) and old earnings")]
    public void NewPaymentsEntriesAreCreatedAndMarkedAsNotSentForPaymentInTheDurableEntityWithTheDifferenceBetweenNewEarningsAndOldEarnings(int expectedNewEarningAmount)
    {
        var testData = _context.Get<TestData>();

        testData.NewEarningsAmount = expectedNewEarningAmount;

        var previousEarningsAmount = testData.EarningsGeneratedEvent.DeliveryPeriods[0].LearningAmount;

        var newStartDateCollectionPeriod = TableExtensions.Period[testData.NewStartDate.ToString("MMMM")];
        var newStartDateCollectionYear = TableExtensions.CalculateAcademicYear("0", testData.NewStartDate);

        //work out when the overlapping period starts where a diff payment needs to be created (will be difference depending on if the start date has moved backwards or forwards)
        var overlappingPeriodStart = testData.OriginalStartDate.GetValueOrDefault() < testData.NewStartDate ? testData.NewStartDate : testData.OriginalStartDate.GetValueOrDefault();

        var expectedPaymentPeriods = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
            new Period(overlappingPeriodStart),
            new Period(short.Parse(testData.CurrentCollectionYear), testData.CurrentCollectionPeriod),
            new PaymentExpectation
            {
                Amount = testData.NewEarningsAmount - previousEarningsAmount,
                SentForPayment = false
            });

        //Validate PaymentsGenerateEvent and payments entity

        expectedPaymentPeriods.AssertAgainstEventPayments(testData.PaymentsGeneratedEvent.Payments);

        expectedPaymentPeriods.AssertAgainstEntityArray(testData.PaymentDbRecords);
    }

    [Then(@"for all payments for future collection periods are equal to the new earnings")]
    public void ThenForAllPaymentsForFutureCollectionPeriodsAreEqualToTheNewEarnings()
    {
        var testData = _context.Get<TestData>();
        var firstDeliveryPeriod = testData.NewStartDate.GetYearMonthFirstDay() > DateTime.Now.GetYearMonthFirstDay() ? new Period(testData.NewStartDate) : new Period(DateTime.Now).GetNextPeriod();//latest of new start date vs current collection period

        var paymentPeriodExpectations = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
            firstDeliveryPeriod, 
            new Period(testData.PlannedEndDate!.Value).GetPreviousPeriod(),
            new PaymentExpectation
            {
                Amount = testData.NewEarningsAmount,
                SentForPayment = false
            });

        //validate Payments Generated Event & Entity

        paymentPeriodExpectations.AssertAgainstEventPayments(testData.PaymentsGeneratedEvent.Payments);

        paymentPeriodExpectations.AssertAgainstEntityArray(testData.PaymentDbRecords);
    }

    [Then(@"for all payments for past collection periods before the original start date \(new start date has moved backwards\) are equal to the new earnings")]
    public void ThenForAllPaymentsForPastCollectionPeriodsBeforeOriginalStartDateAreEqualToTheNewEarnings()
    {
        var testData = _context.Get<TestData>();
        if (testData.NewStartDate > testData.OriginalStartDate || testData.NewStartDate > DateTime.Now) return; //no past periods this step applies to if new start date is not in the past and not before original start date

        var paymentPeriodExpectations = PaymentDeliveryPeriodExpectationBuilder.BuildForDeliveryPeriodRange(
            new Period(testData.NewStartDate),
            new Period(testData.OriginalStartDate.Value).GetPreviousPeriod(),
            new PaymentExpectation
            {
                Amount = testData.NewEarningsAmount,
                SentForPayment = false
            });

        //validate Payments Generated Event  &Entity

        foreach (var periodExpectation in paymentPeriodExpectations)
        {
            Assert.That(testData.PaymentsGeneratedEvent.Payments.Any(x => x.AcademicYear == periodExpectation.DeliveryPeriod.AcademicYear && x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue && x.Amount == periodExpectation.Expectation.Amount),
                $"Expected Amount for delivery period {periodExpectation.DeliveryPeriod.AcademicYear}-{periodExpectation.DeliveryPeriod.PeriodValue} to be {periodExpectation.Expectation.Amount} but was {testData.PaymentsGeneratedEvent.Payments.FirstOrDefault(x => x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue)?.Amount} " +
                $" in Payments Generated Event post CoP - Future delivery periods");

            Assert.That(testData.PaymentDbRecords.Any(x => x.AcademicYear == periodExpectation.DeliveryPeriod.AcademicYear && x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue && (decimal)x.Amount == periodExpectation.Expectation.Amount),
                $"Expected Amount for delivery period {periodExpectation.DeliveryPeriod.AcademicYear}-{periodExpectation.DeliveryPeriod.PeriodValue} to be {periodExpectation.Expectation.Amount} but was {testData.PaymentDbRecords.FirstOrDefault(x => x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue)?.Amount} " +
                $" in Durable Entity - Future delivery periods");

            Assert.That(testData.PaymentDbRecords.Any(x => x.AcademicYear == periodExpectation.DeliveryPeriod.AcademicYear && x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue && x.SentForPayment == periodExpectation.Expectation.SentForPayment),
                $"Expected SentForPayment flag for delivery period {periodExpectation.DeliveryPeriod.AcademicYear}-{periodExpectation.DeliveryPeriod.PeriodValue} to be {periodExpectation.Expectation.SentForPayment} but was {testData.PaymentDbRecords.FirstOrDefault(x => x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue)?.SentForPayment} " +
                $" in Durable Entity - Future delivery periods");
        }
    }

    [Then(@"earnings prior to (.*) and (.*) are frozen with (.*)")]
    public void EarningsPriorToAndAreFrozenWith(int delivery_period, string academicYearString, double oldInstalmentAmount)
    {
        var academicYear = TableExtensions.GetAcademicYear(academicYearString);

        var earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

        var newEarningsProfile = earningsApprenticeshipModel.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).EarningsProfile.Instalments;

        for (int i = 1; i < delivery_period; i++)
        {
            var instalment =
                newEarningsProfile.Single(x => x.AcademicYear.ToString() == academicYear && x.DeliveryPeriod == i);

            Assert.AreEqual(oldInstalmentAmount, instalment.Amount, $"Earning prior to DeliveryPeriod {delivery_period} " +
                                                                               $" are not frozen. Expected Amount for Delivery Period: {instalment.DeliveryPeriod} and AcademicYear: " +
                                                                               $" {instalment.AcademicYear} to be {oldInstalmentAmount} but was {instalment.Amount}");
        }
    }

    [Then(@"the history of old earnings is maintained with (.*)")]
    public async Task HistoryOfOldEarningsIsMaintained(double old_instalment_amount)
    {
        await WaitHelper.WaitForIt(() =>
        {
            var earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

            var historicalInstalments = earningsApprenticeshipModel?.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate)?.StartDate)?.EarningsProfileHistory.FirstOrDefault()?.Instalments;

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
        var testData = _context.Get<TestData>();
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
                    StartDate = testData.OriginalStartDate.GetValueOrDefault(),
                    EndDate = testData.PlannedEndDate.GetValueOrDefault(),
                    TrainingPrice = decimal.Parse(table.Rows[0]["training_price"]),
                    FundingBandMaximum = (int)Math.Ceiling(testData.FundingBandMax),
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