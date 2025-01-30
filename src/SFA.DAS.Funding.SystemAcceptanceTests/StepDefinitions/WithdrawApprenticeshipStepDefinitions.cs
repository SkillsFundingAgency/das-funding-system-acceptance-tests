using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System.Linq;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
internal class WithdrawApprenticeshipStepDefinitions
{
    private readonly ScenarioContext _context;
    private Helpers.Sql.Apprenticeship? apprenticeship;
    private readonly EarningsRecalculatedEventHelper _earningsRecalculatedEventHelper;
    private EarningsApprenticeshipModel? _earningsApprenticeshipModel;
    private ApprenticeshipEarningsRecalculatedEvent _recalculatedEarningsEvent;

    public WithdrawApprenticeshipStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _earningsRecalculatedEventHelper = new EarningsRecalculatedEventHelper(_context);
    }

    [When(@"the apprenticeship is withdrawn")]
    public async Task WithdrawApprenticeship()
    {
        var apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var apprenticeshipsClient = new ApprenticeshipsClient();
        var body = new WithdrawApprenticeshipRequestBody
        {
            UKPRN = apprenticeshipCreatedEvent.Episode.Ukprn,
            ULN = apprenticeshipCreatedEvent.Uln,
            Reason = "WithdrawFromBeta",
            ReasonText = "",
            LastDayOfLearning = commitmentsApprenticeshipCreatedEvent.ActualStartDate!.Value.AddDays(1),
            ProviderApprovedBy = "SystemAcceptanceTest"
        };

        await apprenticeshipsClient.WithdrawApprenticeship(body);
    }

    [When("a Withdrawal request is recorded with a reason (.*) and last day of delivery (.*)")]
    public async Task WithdrawalRequestIsRecordedWithAReasonWithdrawDuringLearningAndLastDayOfDelivery(string reason, DateTime lastDayOfDelivery)
    {
        var apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var apprenticeshipsClient = new ApprenticeshipsClient();
        var body = new WithdrawApprenticeshipRequestBody
        {
            UKPRN = apprenticeshipCreatedEvent.Episode.Ukprn,
            ULN = apprenticeshipCreatedEvent.Uln,
            Reason = reason,
            ReasonText = "",
            LastDayOfLearning = lastDayOfDelivery,
            ProviderApprovedBy = "SystemAcceptanceTest"
        };

        await apprenticeshipsClient.WithdrawApprenticeship(body);
    }


    [Then(@"the apprenticeship is marked as withdrawn")]
    public async Task ApprenticeshipIsMarkedAsWithdrawn()
    {
        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey); 

        await WaitHelper.WaitForIt(() =>
        {
            apprenticeship = apprenticeshipSqlClient.GetApprenticeship(apprenticeshipKey);

            return apprenticeship.Episodes.First().LearningStatus == "Withdrawn";
        }, "LearningStatus did not change to 'Withdrawn' in time.");

        Assert.AreEqual(apprenticeship?.WithdrawalRequests.Count, 1);
    }

    [Then("earnings are recalculated")]
    public async Task EarningsAreRecalculated()
    {
        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);

        await _earningsRecalculatedEventHelper.ReceiveEarningsRecalculatedEvent(apprenticeshipKey);

        _recalculatedEarningsEvent = _context.Get<ApprenticeshipEarningsRecalculatedEvent>();

        _earningsApprenticeshipModel = new EarningsSqlClient().GetEarningsEntityModel(_context);
    }

    [Then("the expected number of earnings instalments after withdrawal are (.*)")]
    public void ExpectedNumberOfEarningsInstalmentsAfterWithdrawalIs(int expectedInstalmentsNumber)
    {
        Assert.AreEqual(expectedInstalmentsNumber, _recalculatedEarningsEvent.DeliveryPeriods.Count, "Unexpected number of instalments in earnings recalculated event");

        var actualInstalmentsNumber = _earningsApprenticeshipModel?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.Count ?? 0;
        Assert.AreEqual(expectedInstalmentsNumber, actualInstalmentsNumber, "Unexpected number of instalments after withdrawal has been recorded in earnings db!");
    }

    [Then("the earnings after the delivery period (.*) and academic year (.*) are soft deleted")]
    public void EarningsAfterTheDeliveryPeriodAndAcademicYearAreSoftDeleted(string deliveryPeriod, string academicYear)
    {
        if (deliveryPeriod != null && academicYear != null)
        {
            bool isValidRecalculatedEarnings = _recalculatedEarningsEvent.DeliveryPeriods?
                .All(Dp => Dp.AcademicYear < Convert.ToInt16(academicYear) 
                || (Dp.AcademicYear == Convert.ToInt16(academicYear) && Dp.Period <= Convert.ToInt16(deliveryPeriod))) ?? true;

            Assert.IsTrue(isValidRecalculatedEarnings, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear} in recalculated earnings event.");


            bool isValidEarningInDb = _earningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?
               .All(i => i.AcademicYear < Convert.ToInt16(academicYear) 
               || (i.AcademicYear == Convert.ToInt16(academicYear) && i.DeliveryPeriod <= Convert.ToInt16(deliveryPeriod))) ?? true;

            Assert.IsTrue(isValidEarningInDb, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear} in earnings db.");
        }
    }
}
