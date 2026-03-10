using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
internal class WithdrawApprenticeshipStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly LearningSqlClient _apprenticeshipSqlClient;
    private readonly EarningsSqlClient _earningsSqlClient;
    public WithdrawApprenticeshipStepDefinitions(
        ScenarioContext context,
        LearningSqlClient apprenticeshipSqlClient,
        EarningsSqlClient earningsSqlClient)
    {
        _context = context;
        _apprenticeshipSqlClient = apprenticeshipSqlClient;
        _earningsSqlClient = earningsSqlClient;
    }

    [Given(@"the apprenticeship is marked as withdrawn")]
    [When(@"the apprenticeship is marked as withdrawn")]
    [Then(@"the apprenticeship is marked as withdrawn")]
    public async Task ApprenticeshipIsMarkedAsWithdrawn()
    {
        var testData = _context.Get<TestData>();
        SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql.Learning? apprenticeship = null;

        await WaitHelper.WaitForIt(() =>
        {
            apprenticeship = _apprenticeshipSqlClient.GetApprenticeship(testData.LearningKey);

            return apprenticeship.Episodes.First().WithdrawalDate == testData.LastDayOfLearning;
        }, $"Incorrect LastDayOfLearning found in the Learning db, Episode table.");
    }

    [Given("last day of learning is set to (.*) in learning and earning db")]
    [Then("last day of learning is set to (.*) in learning and earning db")]
    public async Task LastDayOfLearningIsSetToDateInLearningDb(TokenisableDateTime withdrawalDate)
    {
        var testData = _context.Get<TestData>();
        SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql.Learning? apprenticeship = null;

        await WaitHelper.WaitForIt(() =>
        {
            apprenticeship = _apprenticeshipSqlClient.GetApprenticeship(testData.LearningKey);
            var earnings  = _earningsSqlClient.GetEarningsEntityModel(_context);

            return apprenticeship.Episodes.First().WithdrawalDate == withdrawalDate.Value && earnings.Episodes.First().WithdrawalDate == withdrawalDate.Value;
        }, $"LastDayOfLearning did not change to {withdrawalDate} in learning db episode table");
    }

    [Given("earnings are recalculated")]
    [When("earnings are recalculated")]
    [Then("earnings are recalculated")]
    public async Task EarningsAreRecalculated()
    {
        var testData = _context.Get<TestData>();
        await _context.ReceiveEarningsRecalculatedEvent(testData.LearningKey);
        testData.EarningsApprenticeshipModel = _earningsSqlClient.GetEarningsEntityModel(_context);
    }

    [Given("the expected number of earnings instalments after withdrawal are (.*)")]
    [When("the expected number of earnings instalments after withdrawal are (.*)")]
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

    [When("the earnings after the delivery period (.*) and academic year (.*) are soft deleted")]
    [Then("the earnings after the delivery period (.*) and academic year (.*) are soft deleted")]
    public void EarningsAfterTheDeliveryPeriodAndAcademicYearAreSoftDeleted(string deliveryPeriod, TokenisableAcademicYear academicYear)
    {
        var testData = _context.Get<TestData>();

        if (deliveryPeriod != "null")
        {
            bool isValidEarningInDb = testData.EarningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?
               .All(i => i.AcademicYear < academicYear.Value
               || (i.AcademicYear == academicYear.Value && i.DeliveryPeriod <= Convert.ToInt16(deliveryPeriod))) ?? true;

            Assert.IsTrue(isValidEarningInDb, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear} in earnings db.");
        }
    }

    [Given("Learning withdrawal date is recorded on (.*)")]
    [When("Learning withdrawal date is recorded on (.*)")]
    public void LearningWithdrawalDateIsRecordedOn(TokenisableDateTime? withdrawalDate)
    {
        var testData = _context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithWithdrawalDate(withdrawalDate.Value);

        testData.LastDayOfLearning = withdrawalDate.Value;
    }

}
