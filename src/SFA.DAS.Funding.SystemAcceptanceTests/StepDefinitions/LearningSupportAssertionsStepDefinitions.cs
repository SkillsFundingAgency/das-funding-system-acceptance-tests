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
    private readonly PaymentsSqlClient _paymentsSqlClient;
    private readonly EarningsInnerApiHelper _earningsInnerApiHelper;

    public LearningSupportAssertionsStepDefinitions(
        ScenarioContext context,
        EarningsSqlClient earningsEntitySqlClient, 
        PaymentsSqlClient paymentsSqlClient,
        EarningsInnerApiHelper earningsInnerApiHelper)
    {
        _context = context;
        _earningsEntitySqlClient = earningsEntitySqlClient;
        _paymentsSqlClient = paymentsSqlClient;
        _earningsInnerApiHelper = earningsInnerApiHelper;
    }

    [When(@"learning support is recorded from (.*) to (.*)")]
    public async Task AddLearningSupport(TokenisableDateTime learningSupportStart, TokenisableDateTime learningSupportEnd)
    {
        var testData = _context.Get<TestData>();
        PaymentsGeneratedEventHandler.ReceivedEvents.Clear();
        await _earningsInnerApiHelper.SetLearningSupportPayments(testData.ApprenticeshipKey,
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

        while (learningSupportStart.Value.AcademicYear < learningSupportEnd.Value.AcademicYear || (learningSupportStart.Value.AcademicYear == learningSupportEnd.Value.AcademicYear && learningSupportStart.Value.PeriodValue <= learningSupportEnd.Value.PeriodValue))
        {
            additionalPayments.Should().Contain(x =>
                    x.AdditionalPaymentType == AdditionalPaymentType.LearningSupport
                    && x.Amount == 150
                    && x.AcademicYear == learningSupportStart.Value.AcademicYear
                    && x.DeliveryPeriod == learningSupportStart.Value.PeriodValue, $"Expected learning support earning for {learningSupportStart.Value.ToCollectionPeriodString()}");

            learningSupportStart.Value = learningSupportStart.Value.GetNextPeriod();
        }
    }

    [Then(@"learning support payments are generated from periods (.*) to (.*)")]
    public async Task VerifyLearningSupportPayments(TokenisablePeriod learningSupportStart, TokenisablePeriod learningSupportEnd)
    {
        var testData = _context.Get<TestData>();
        PaymentsApprenticeshipModel? paymentsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            paymentsApprenticeshipModel = _paymentsSqlClient.GetPaymentsModel(_context);
            return paymentsApprenticeshipModel?.Earnings.Any(x => x.EarningsProfileId == testData.EarningsProfileId) ?? false;
        }, "Failed to find updated payments entity.");

        await _context.ReceivePaymentsEvent(testData.ApprenticeshipKey);

        while (learningSupportStart.Value.AcademicYear < learningSupportEnd.Value.AcademicYear || (learningSupportStart.Value.AcademicYear == learningSupportEnd.Value.AcademicYear && learningSupportStart.Value.PeriodValue <= learningSupportEnd.Value.PeriodValue))
        {
            testData.PaymentsGeneratedEvent.Payments.Should().Contain(x =>
                    x.PaymentType == AdditionalPaymentType.LearningSupport.ToString()
                    && x.Amount == 150
                    && x.AcademicYear == learningSupportStart.Value.AcademicYear
                    && x.DeliveryPeriod == learningSupportStart.Value.PeriodValue, $"Expected learning support earning for {learningSupportStart.Value.ToCollectionPeriodString()}");

            learningSupportStart.Value = learningSupportStart.Value.GetNextPeriod();
        }
    }
}