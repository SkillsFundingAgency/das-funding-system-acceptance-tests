using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
internal class WithdrawApprenticeshipStepDefinitions
{
    private readonly ScenarioContext _context;

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
            ProviderApprovedBy = "Test"
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
}
