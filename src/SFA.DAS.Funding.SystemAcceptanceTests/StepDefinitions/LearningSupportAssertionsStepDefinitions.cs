using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
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
    private Guid _earningsProfileId;
    private bool _learningSupportAdded = false;

    public LearningSupportAssertionsStepDefinitions(ScenarioContext context, EarningsSqlClient earningsEntitySqlClient)
    {
        _context = context;
        _earningsEntitySqlClient = earningsEntitySqlClient;
    }

    [When(@"learning support is recorded from (.*) to (.*)")]
    public async Task AddLearningSupport(TokenisableDateTime learningSupportStart, TokenisableDateTime learningSupportEnd)
    {
        PaymentsGeneratedEventHandler.ReceivedEvents.Clear();
        var helper = new EarningsInnerApiHelper();
        await helper.SetLearningSupportPayments(_context.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey,
            [new EarningsInnerApiClient.LearningSupportPaymentDetail() { StartDate = learningSupportStart.Value, EndDate = learningSupportEnd.Value }]);
        _learningSupportAdded = true;
    }

    [Then(@"learning support earnings are generated from periods (.*) to (.*)")]
    public async Task VerifyLearningSupportEarnings(TokenisablePeriod learningSupportStart, TokenisablePeriod learningSupportEnd)
    {
        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);
            return !_learningSupportAdded || earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfileHistory.Any();
        }, "Failed to find updated earnings entity.");

        var additionalPayments = earningsApprenticeshipModel
            .Episodes
            .SingleOrDefault()
            ?.AdditionalPayments;

        _earningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile.EarningsProfileId;

        additionalPayments.Should().NotBeNull("No episode found on earnings apprenticeship model");

        while (learningSupportStart.Value.AcademicYear < learningSupportEnd.Value.AcademicYear || learningSupportStart.Value.AcademicYear >= learningSupportEnd.Value.AcademicYear && learningSupportStart.Value.PeriodValue <= learningSupportEnd.Value.PeriodValue)
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
        PaymentsApprenticeshipModel? paymentsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            paymentsApprenticeshipModel = new PaymentsSqlClient().GetPaymentsModel(_context);
            return paymentsApprenticeshipModel.Earnings.Any(x => x.EarningsProfileId == _earningsProfileId);
        }, "Failed to find updated payments entity.");

        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);
        await _context.ReceivePaymentsEvent(apprenticeshipKey);

        var paymentsGeneratedEvent = _context.Get<PaymentsGeneratedEvent>();

        while (learningSupportStart.Value.AcademicYear < learningSupportEnd.Value.AcademicYear || learningSupportStart.Value.AcademicYear >= learningSupportEnd.Value.AcademicYear && learningSupportStart.Value.PeriodValue <= learningSupportEnd.Value.PeriodValue)
        {
            paymentsGeneratedEvent.Payments.Should().Contain(x =>
                    x.PaymentType == AdditionalPaymentType.LearningSupport.ToString()
                    && x.Amount == 150
                    && x.AcademicYear == learningSupportStart.Value.AcademicYear
                    && x.DeliveryPeriod == learningSupportStart.Value.PeriodValue, $"Expected learning support earning for {learningSupportStart.Value.ToCollectionPeriodString()}");

            learningSupportStart.Value = learningSupportStart.Value.GetNextPeriod();
        }
    }
}