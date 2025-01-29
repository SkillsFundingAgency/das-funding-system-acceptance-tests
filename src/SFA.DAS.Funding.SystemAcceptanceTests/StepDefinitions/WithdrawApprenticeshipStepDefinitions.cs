using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using System.Runtime.CompilerServices;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
internal class WithdrawApprenticeshipStepDefinitions
{
    private readonly ScenarioContext _context;
    private EarningsApprenticeshipModel? _earningsApprenticeshipModel;

    public WithdrawApprenticeshipStepDefinitions(ScenarioContext context)
    {
        _context = context;
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
    public void ApprenticeshipIsMarkedAsWithdrawn()
    {
        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);
        var apprenticeship = apprenticeshipSqlClient.GetApprenticeship(apprenticeshipKey);

        Assert.AreEqual(apprenticeship.Episodes.First().LearningStatus, "Withdrawn");
        Assert.AreEqual(apprenticeship.WithdrawalRequests.Count, 1);
    }

    [Then("earnings are recalculated")]
    public void EarningsAreRecalculated()
    {
        var earningsSqlClient = new EarningsSqlClient();
        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);
        _earningsApprenticeshipModel = earningsSqlClient.GetEarningsEntityModel(_context);
    }

    [Then("the expected number of earnings instalments after withdrawal are (.*)")]
    public void ExpectedNumberOfEarningsInstalmentsAfterWithdrawalIs(int expectedInstalmentsNumber)
    {
        var actualInstalmentsNumber = _earningsApprenticeshipModel?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.Count;
        Assert.AreEqual(expectedInstalmentsNumber, actualInstalmentsNumber, "Unexpected number of instalments after withdrawal has been recorded!");
    }

    [Then("the earnings after the delivery period (.*) and academic year (.*) are soft deleted")]
    public void EarningsAfterTheDeliveryPeriodAndAcademicYearAreSoftDeleted(int deliveryPeriod, int academicYear)
    {
        bool isValid = _earningsApprenticeshipModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?
               .All(i => i.DeliveryPeriod <= deliveryPeriod || i.AcademicYear <= academicYear) ?? true;

        Assert.IsTrue(isValid, $"Some instalments have a delivery period > {deliveryPeriod} and academic year > {academicYear}.");
    }
}
