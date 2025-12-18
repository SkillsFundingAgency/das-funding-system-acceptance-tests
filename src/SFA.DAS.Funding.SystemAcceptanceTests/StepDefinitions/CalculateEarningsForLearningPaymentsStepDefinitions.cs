using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class CalculateEarningsForLearningPaymentsStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly EarningsSqlClient _earningsSqlClient;
    
    public CalculateEarningsForLearningPaymentsStepDefinitions(ScenarioContext context, EarningsSqlClient earningsSqlClient)
    {
        _context = context;
        _earningsSqlClient = earningsSqlClient;
    }

    [When(@"the agreed price is (below|above) the funding band maximum for the selected course")]
    public void VerifyFundingBandMaxValue(string condition)
    {
        var testData = _context.Get<TestData>();
        var commitmentsApprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;
        var earnings = _earningsSqlClient.GetEarningsEntityModel(_context);

        if (condition == "below") Assert.Less(commitmentsApprenticeshipCreatedEvent.PriceEpisodes.MaxBy(x => x.FromDate)!.Cost, 
            earnings?.Episodes.FirstOrDefault()?.FundingBandMaximum);
        else Assert.Greater(commitmentsApprenticeshipCreatedEvent.PriceEpisodes.MaxBy(x => x.FromDate)!.Cost,
            earnings?.Episodes.FirstOrDefault()?.FundingBandMaximum);
    }

    [Then(@"80% of the agreed price is calculated as total on-program payment which is divided equally into number of planned months (.*)")]
    [Then(@"Agreed price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
    [Then(@"Funding band maximum price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
    public void VerifyInstalmentAmountIsCalculatedEquallyIntoAllEarningMonths(decimal instalmentAmount)
    {
        var testData = _context.Get<TestData>();
        var deliveryPeriods = testData.EarningsGeneratedEvent.DeliveryPeriods;
        deliveryPeriods.FilterByOnProg().ToList().ForEach(dp => dp.LearningAmount.Should().Be(instalmentAmount));
    }

    [Given(@"the planned number of months must be the number of months from the start date to the planned end date (.*)")]
    [Then(@"the planned number of months must be the number of months from the start date to the planned end date (.*)")]
    public void VerifyThePlannedDurationMonthsWithinTheEarningsGenerated(short numberOfInstalments)
    {
        var testData = _context.Get<TestData>();
        var deliveryPeriods = testData.EarningsGeneratedEvent.DeliveryPeriods;
        deliveryPeriods.FilterByOnProg().Should().HaveCount(numberOfInstalments);
    }

    [Given(@"the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month/year")]
    [Then(@"the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month/year")]
    public void ThenTheDeliveryPeriodForEachInstalmentMustBeTheDeliveryPeriodFromTheCollectionCalendarWithAMatchingCalendarMonthYear(Table table)
    {
        var testData = _context.Get<TestData>();
        var deliveryPeriods = testData.EarningsGeneratedEvent.DeliveryPeriods;
        deliveryPeriods.FilterByOnProg().ToList().ShouldHaveCorrectFundingPeriods(table.ToExpectedPeriods());
    }

    [Then(@"the total completion amount (.*) should be calculated as 20% of the adjusted price")]
    public void VerifyCompletionAmountIsCalculatedCorrectly(decimal completionAmount)
    {
        var apprenticeshipEntity = _earningsSqlClient.GetEarningsEntityModel(_context);

        Assert.AreEqual(completionAmount, apprenticeshipEntity!.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate)!.StartDate)!.EarningsProfile.CompletionPayment);
    }

    [Then(@"the leaners age (.*) at the start of the course and funding line type (.*) must be calculated")]
    public void ValidateAgeAndFundingLineTypeCalculated(int age, string fundingLineType)
    {
        var testData = _context.Get<TestData>();
        Assert.AreEqual(testData.LearningCreatedEvent.Episode.AgeAtStartOfLearning, age, $"Expected age is: {age} but found age: {testData.LearningCreatedEvent.Episode.AgeAtStartOfLearning}");

        var deliveryPeriods = testData.EarningsGeneratedEvent.DeliveryPeriods;
        deliveryPeriods.FilterByOnProg().ToList().ShouldHaveCorrectFundingLineType(fundingLineType);
    }
}
