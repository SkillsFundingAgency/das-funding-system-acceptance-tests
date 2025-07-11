using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class LearningSupportAssertionsStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly EarningsSqlClient _earningsEntitySqlClient;
    private readonly EarningsInnerApiHelper _earningsInnerApiHelper;

    public LearningSupportAssertionsStepDefinitions(
        ScenarioContext context,
        EarningsSqlClient earningsEntitySqlClient, 
        EarningsInnerApiHelper earningsInnerApiHelper)
    {
        _context = context;
        _earningsEntitySqlClient = earningsEntitySqlClient;
        _earningsInnerApiHelper = earningsInnerApiHelper;
    }

    [When(@"learning support is recorded from (.*) to (.*)")]
    public async Task AddLearningSupport(TokenisableDateTime learningSupportStart, TokenisableDateTime learningSupportEnd)
    {
        var testData = _context.Get<TestData>();
        await _earningsInnerApiHelper.SetLearningSupportPayments(testData.LearningKey,
            [new EarningsInnerApiClient.LearningSupportPaymentDetail() { StartDate = learningSupportStart.Value, EndDate = learningSupportEnd.Value }]);
        testData.IsLearningSupportAdded = true;
    }

    [Then(@"learning support earnings are generated from periods (.*) to (.*)")]
    public async Task VerifyLearningSupportEarnings(TokenisablePeriod learningSupportStart, TokenisablePeriod learningSupportEnd)
    {
        var testData = _context.Get<TestData>();
        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);
            return !testData.IsLearningSupportAdded || earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfileHistory.Any();
        }, "Failed to find updated earnings entity.");

        var additionalPayments = earningsApprenticeshipModel
            .Episodes
            .SingleOrDefault()
            ?.AdditionalPayments;

        testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile.EarningsProfileId;

        additionalPayments.Should().NotBeNull("No episode found on earnings apprenticeship model");

        additionalPayments.Should().NotContain(x =>
                new Period(x.AcademicYear, x.DeliveryPeriod).IsBefore(learningSupportStart.Value),
            $"Expected no Learning Support earnings before {learningSupportStart.Value.ToCollectionPeriodString()}");

        additionalPayments.Should().NotContain(x =>
                learningSupportEnd.Value.IsBefore(new Period(x.AcademicYear, x.DeliveryPeriod)),
            $"Expected no Learning Support earnings after {learningSupportEnd.Value.ToCollectionPeriodString()}");

        while (learningSupportStart.Value.IsBefore(learningSupportEnd.Value))
        {
            additionalPayments.Should().Contain(x =>
                    x.AdditionalPaymentType == AdditionalPaymentType.LearningSupport
                    && x.Amount == 150
                    && x.AcademicYear == learningSupportStart.Value.AcademicYear
                    && x.DeliveryPeriod == learningSupportStart.Value.PeriodValue, $"Expected learning support earning for {learningSupportStart.Value.ToCollectionPeriodString()}");

            learningSupportStart.Value = learningSupportStart.Value.GetNextPeriod();
        }
    }
}