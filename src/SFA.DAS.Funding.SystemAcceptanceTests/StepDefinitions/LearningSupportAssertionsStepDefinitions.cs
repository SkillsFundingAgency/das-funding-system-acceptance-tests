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
        PaymentsGeneratedEventHandler.Clear(x => x.ApprenticeshipKey == testData.ApprenticeshipKey);
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

    [Then(@"learning support payments are generated from periods (.*) to (.*)")]
    public async Task VerifyLearningSupportPayments(TokenisablePeriod learningSupportStart, TokenisablePeriod learningSupportEnd)
    {
        var testData = _context.Get<TestData>();
        PaymentsApprenticeshipModel? paymentsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            paymentsApprenticeshipModel = _paymentsSqlClient.GetPaymentsModel(_context);
          
            if (paymentsApprenticeshipModel == null || paymentsApprenticeshipModel.Earnings == null)
            {
                return false;
            }

            return paymentsApprenticeshipModel.Earnings.Any(x => x.EarningsProfileId == testData.EarningsProfileId);
        }, "Failed to find updated payments entity.");

        try
        {
            testData.PaymentsGeneratedEvent.Payments.Should().NotContain(x =>
                    x.PaymentType == AdditionalPaymentType.LearningSupport.ToString()
                    && new Period(x.AcademicYear, x.DeliveryPeriod).IsBefore(learningSupportStart.Value),
                $"Expected no Learning Support payments before {learningSupportStart.Value.ToCollectionPeriodString()}");

            testData.PaymentsGeneratedEvent.Payments.Should().NotContain(x =>
                    x.PaymentType == AdditionalPaymentType.LearningSupport.ToString()
                    && learningSupportEnd.Value.IsBefore(new Period(x.AcademicYear, x.DeliveryPeriod)),
                $"Expected no Learning Support payments after {learningSupportEnd.Value.ToCollectionPeriodString()}");

            while (learningSupportStart.Value.IsBefore(learningSupportEnd.Value))
            {
                testData.PaymentsGeneratedEvent.Payments.Should().Contain(x =>
                        x.PaymentType == AdditionalPaymentType.LearningSupport.ToString()
                        && x.Amount == 150
                        && x.AcademicYear == learningSupportStart.Value.AcademicYear
                        && x.DeliveryPeriod == learningSupportStart.Value.PeriodValue, $"Expected learning support earning for {learningSupportStart.Value.ToCollectionPeriodString()}");

                learningSupportStart.Value = learningSupportStart.Value.GetNextPeriod();
            }
        }
        catch(Exception ex)
        {
            throw new Exception($"Failed to verify learning support payments. {ex.Message}");
        }
    }

    [Then("no Maths and English learning support payments are generated")]
    public void NoMathsAndEnglishLearningSupportPaymentsAreGenerated()
    {
        var testData = _context.Get<TestData>();

        testData.PaymentsGeneratedEvent.Payments
                .Where(x => x.PaymentType == AdditionalPaymentType.LearningSupport.ToString())
                .Should()
                .BeEmpty("No Learning Support payments should be present");
    }
}