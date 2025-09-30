using Newtonsoft.Json;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class RecalculateEarningsStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly EarningsSqlClient _earningsEntitySqlClient;
    private readonly ApprenticeshipsInnerApiHelper _apprenticeshipsInnerApiHelper;

    public RecalculateEarningsStepDefinitions(
        ScenarioContext context, 
        EarningsSqlClient earningsSqlClient,
        ApprenticeshipsInnerApiHelper apprenticeshipsInnerApiHelper)
    {
        _context = context;
        _earningsEntitySqlClient = earningsSqlClient;
        _apprenticeshipsInnerApiHelper = apprenticeshipsInnerApiHelper;
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

        await _apprenticeshipsInnerApiHelper.CreatePriceChangeRequest(_context, testData.NewTrainingPrice, testData.NewAssessmentPrice, testData.PriceChangeEffectiveFrom);
        await _apprenticeshipsInnerApiHelper.ApprovePendingPriceChangeRequest(_context, testData.NewTrainingPrice, testData.NewAssessmentPrice, testData.PriceChangeApprovedDate);
    }

    [When(@"the start date change is approved")]
    public async Task StartDateChangeIsApproved()
    {
        var testData = _context.Get<TestData>();
        var startDateChangedEvent = ApprenticeshipStartDateChangedEventHelper.CreateStartDateChangedMessageWithCustomValues(_context, testData.NewStartDate, testData.NewEndDate, testData.StartDateChangeApprovedDate);
        await ApprenticeshipStartDateChangedEventHelper.PublishApprenticeshipStartDateChangedEvent(startDateChangedEvent);
    }

    [Then(@"the earnings are recalculated based on the new instalment amount of (.*) from (.*) and (.*)")]
    public async Task EarningsAreRecalculatedBasedOnTheNewInstalmentAmountOfFromAnd(decimal newInstalmentAmount, int deliveryPeriod, string academicYearString)
    {
        var testData = _context.Get<TestData>();
        var academicYear = TableExtensions.GetAcademicYear(academicYearString);

        await _context.ReceiveEarningsRecalculatedEvent(testData.LearningCreatedEvent.LearningKey);

        testData.ApprenticeshipEarningsRecalculatedEvent!.DeliveryPeriods.Where(Dp => Dp.AcademicYear == Convert.ToInt16(academicYear) && Dp.Period >= deliveryPeriod).All(p => p.LearningAmount.Should().Equals(newInstalmentAmount));
        testData.ApprenticeshipEarningsRecalculatedEvent!.DeliveryPeriods.Where(Dp => Dp.AcademicYear > Convert.ToInt16(academicYear)).All(p => p.LearningAmount.Should().Equals(newInstalmentAmount));
    }

    [Then(@"the earnings are recalculated based on the new expected earnings (.*)")]
    public async Task EarningsAreRecalculatedBasedOnTheNewExpectedEarnings(decimal newInstalmentAmount)
    {
        var testData = _context.Get<TestData>();

        await _context.ReceiveEarningsRecalculatedEvent(testData.LearningKey);

        var deliveryPeriods = testData.ApprenticeshipEarningsRecalculatedEvent.DeliveryPeriods;

        for (var i = 0; i < deliveryPeriods.Count; i++)
        {
            Assert.AreEqual(deliveryPeriods[i].LearningAmount, newInstalmentAmount, $"Expected new instalment amount to be {newInstalmentAmount} but found {deliveryPeriods[i].LearningAmount}");
        }
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


    [Then(@"old and new earnings maintain their initial Profile Id")]
    public void OldEarningsMaintainTheirInitialProfileId()
    {
        var testData = _context.Get<TestData>();
        var earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

        Assert.AreEqual(testData.InitialEarningsProfileId, earningsApprenticeshipModel.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate)
            .EarningsProfileHistory.FirstOrDefault().EarningsProfileId, "EarningsProfileId in EarningsProfileHistory table does not match the initial EarningsProfileId");
        
        Assert.AreEqual(testData.InitialEarningsProfileId, earningsApprenticeshipModel.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).EarningsProfile.EarningsProfileId, "EarningsProfileId has changed post earnings recalculation");
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

            var historicalInstalmentsString = earningsApprenticeshipModel?.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate)?.StartDate)?
            .EarningsProfileHistory
            .OrderBy(h => h.CreatedOn)
            .FirstOrDefault()?.State;

            if (!string.IsNullOrWhiteSpace(historicalInstalmentsString))
            {
                var historicalInstalments = JsonConvert.DeserializeObject<EarningsProfileUpdatedEvent>(historicalInstalmentsString)?.Instalments;

                if (historicalInstalments != null)
                {
                    foreach (var instalment in historicalInstalments)
                    {
                        Assert.AreEqual(old_instalment_amount, instalment.Amount, $"Expected historical earnings amount to be {old_instalment_amount}, but was {instalment.Amount}");
                    }
                    return true;
                }
                return false;
            }
            else
                return false;
        }, "Failed to find installments in Earnings Profile History");
    }

    [Given(@"a price change event is approved")]
    public async Task GivenAPriceChangeEventIsApproved(Table table)
    {
        var testData = _context.Get<TestData>();
        var fixture = new Fixture();

        var apprenticeshipPriceChangedEvent = fixture.Build<LearningPriceChangedEvent>()
        .With(_ => _.LearningKey, Guid.Parse(table.Rows[0]["apprenticeship_key"]))
        .With(_ => _.ApprovalsApprenticeshipId, long.Parse(table.Rows[0]["apprenticeship_id"]))
        .With(_ => _.Episode, new LearningEpisode
        {
            Prices = new List<LearningEpisodePrice>
            {
                new LearningEpisodePrice
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
            Key = _context.Get<Learning.Types.LearningCreatedEvent>().Episode.Key
        })
        .With(_ => _.EffectiveFromDate, DateTime.Parse(table.Rows[0]["effective_from_date"]))
        .With(_ => _.ApprovedDate, DateTime.Parse(table.Rows[0]["approved_date"]))
        .Create();

        await TestServiceBus.Das.SendPriceChangeApprovedMessage(apprenticeshipPriceChangedEvent);
    }

    [Then("an end date changed event is published to approvals with end date (.*)")]
    public async Task EndDateChangedEventIsPublishedToApprovals(TokenisableDateTime endDate)
    {
        var testData = _context.Get<TestData>();

        await _context.ReceiveEndDateChangedEvent(testData.LearningCreatedEvent.LearningKey);

        Assert.AreEqual(testData.EndDateChangedEvent.PlannedEndDate.Date, endDate.Value, "Unexpected planned end found!");
        Assert.AreEqual(testData.EndDateChangedEvent.ApprovalsApprenticeshipId, testData.LearningCreatedEvent.ApprovalsApprenticeshipId, "Unexpected ApprenticeshipId found!" );
    }



    static int CalculateMonthsDifference(DateTime endDate, DateTime startDate)
    {
        int monthsDifference = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

        if (endDate.Day < startDate.Day) monthsDifference--;

        return monthsDifference;
    }
}