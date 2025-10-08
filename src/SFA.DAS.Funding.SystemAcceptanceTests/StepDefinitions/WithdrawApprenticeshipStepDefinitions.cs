using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
internal class WithdrawApprenticeshipStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly LearningClient _apprenticeshipsClient;
    private readonly LearningSqlClient _apprenticeshipSqlClient;
    private readonly EarningsSqlClient _earningsSqlClient;

    public WithdrawApprenticeshipStepDefinitions(
        ScenarioContext context,
        LearningClient apprenticeshipsClient,
        LearningSqlClient apprenticeshipSqlClient,
        EarningsSqlClient earningsSqlClient)
    {
        _context = context;
        _apprenticeshipsClient = apprenticeshipsClient;
        _apprenticeshipSqlClient = apprenticeshipSqlClient;
        _earningsSqlClient = earningsSqlClient;
    }

    [When(@"the apprenticeship is withdrawn")]
    public async Task WithdrawApprenticeship()
    {
        var testData = _context.Get<TestData>();

        var body = new WithdrawLearningRequestBody
        {
            UKPRN = testData.LearningCreatedEvent.Episode.Ukprn,
            ULN = testData.LearningCreatedEvent.Uln,
            Reason = "WithdrawFromBeta",
            ReasonText = "",
            LastDayOfLearning = testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate!.Value.AddDays(1),
            ProviderApprovedBy = "SystemAcceptanceTest"
        };

        await _apprenticeshipsClient.WithdrawLearning(body);
    }

    [When("a Withdrawal request is recorded with a reason (.*) and last day of delivery (.*)")]
    public async Task WithdrawalRequestIsRecordedWithAReasonWithdrawDuringLearningAndLastDayOfDelivery(string reason, TokenisableDateTime lastDayOfDelivery)
    {
        var testData = _context.Get<TestData>();

        testData.LastDayOfLearning = lastDayOfDelivery.Value;

        var body = new WithdrawLearningRequestBody
        {
            UKPRN = testData.LearningCreatedEvent.Episode.Ukprn,
            ULN = testData.LearningCreatedEvent.Uln,
            Reason = reason,
            ReasonText = "",
            LastDayOfLearning = testData.LastDayOfLearning,
            ProviderApprovedBy = "SystemAcceptanceTest"
        };

        await _apprenticeshipsClient.WithdrawLearning(body);
    }


    [Then(@"the apprenticeship is marked as withdrawn")]
    public async Task ApprenticeshipIsMarkedAsWithdrawn()
    {
        var testData = _context.Get<TestData>();
        SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql.Learning? apprenticeship = null;

        await WaitHelper.WaitForIt(() =>
        {
            apprenticeship = _apprenticeshipSqlClient.GetApprenticeship(testData.LearningKey);

            return apprenticeship.Episodes.First().LearningStatus == "Withdrawn";
        }, "LearningStatus did not change to 'Withdrawn' in time.");
    }

    [Then("last day of learning is set to (.*) in learning db")]
    public async Task LastDayOfLearningIsSetToDateInLearningDb(TokenisableDateTime withdrawalDate)
    {
        var testData = _context.Get<TestData>();
        SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql.Learning? apprenticeship = null;

        await WaitHelper.WaitForIt(() =>
        {
            apprenticeship = _apprenticeshipSqlClient.GetApprenticeship(testData.LearningKey);

            return apprenticeship.Episodes.First().LastDayOfLearning == withdrawalDate.Value;
        }, $"LastDayOfLearning did not change to {withdrawalDate} in learning db episode table");
    }


    [Then("earnings are recalculated")]
    public async Task EarningsAreRecalculated()
    {
        var testData = _context.Get<TestData>();
        await _context.ReceiveEarningsRecalculatedEvent(testData.LearningKey);
        testData.EarningsApprenticeshipModel = _earningsSqlClient.GetEarningsEntityModel(_context);
    }

    [Then("the expected number of earnings instalments after withdrawal are (.*)")]
    public void ExpectedNumberOfEarningsInstalmentsAfterWithdrawalIs(int expectedInstalmentsNumber)
    {
        var testData = _context.Get<TestData>();

        Assert.AreEqual(expectedInstalmentsNumber, testData.ApprenticeshipEarningsRecalculatedEvent.DeliveryPeriods.Count, "Unexpected number of instalments in earnings recalculated event");

        var actualInstalmentsNumber = testData.EarningsApprenticeshipModel.Episodes
            .FirstOrDefault()?
            .EarningsProfile?.Instalments?
            .Where(x => x.Type.Contains("Regular"))
            .Count() ?? 0;

        Assert.AreEqual(expectedInstalmentsNumber, actualInstalmentsNumber, "Unexpected number of instalments after withdrawal has been recorded in earnings db!");
    }

    [Then("the earnings after the delivery period (.*) and academic year (.*) are soft deleted")]
    public void EarningsAfterTheDeliveryPeriodAndAcademicYearAreSoftDeleted(string deliveryPeriod, string academicYear)
    {
        var testData = _context.Get<TestData>();

        if (deliveryPeriod != null && academicYear != null)
        {
            bool isValidRecalculatedEarnings = testData.ApprenticeshipEarningsRecalculatedEvent.DeliveryPeriods?
                .All(Dp => Dp.AcademicYear < Convert.ToInt16(academicYear) 
                || (Dp.AcademicYear == Convert.ToInt16(academicYear) && Dp.Period <= Convert.ToInt16(deliveryPeriod))) ?? true;

            Assert.IsTrue(isValidRecalculatedEarnings, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear} in recalculated earnings event.");


            bool isValidEarningInDb = testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?
               .All(i => i.AcademicYear < Convert.ToInt16(academicYear) 
               || (i.AcademicYear == Convert.ToInt16(academicYear) && i.DeliveryPeriod <= Convert.ToInt16(deliveryPeriod))) ?? true;

            Assert.IsTrue(isValidEarningInDb, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear} in earnings db.");
        }
    }
}
