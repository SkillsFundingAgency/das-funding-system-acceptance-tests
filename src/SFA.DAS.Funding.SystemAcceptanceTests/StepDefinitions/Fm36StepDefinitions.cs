using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class Fm36StepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly EarningsOuterClient _earningsOuterClient;

    public Fm36StepDefinitions(ScenarioContext context)
    {
        _context = context;
        _earningsOuterClient = new EarningsOuterClient();
    }

    [When(@"the fm36 data is retrieved for (.*)")]
    public async Task GetFm36Data(TokenisableDateTime searchDate)
    {
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
        var collectionYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", searchDate.Value));
        var collectionPeriod = TableExtensions.Period[searchDate.Value.ToString("MMMM")];

        var fm36 = await _earningsOuterClient.GetFm36Block(apprenticeshipCreatedEvent.ProviderId, collectionYear, collectionPeriod);
        _context.Set(fm36);
    }

    [Then(@"fm36 data exists for that apprenticeship")]
    public async Task Fm36DataExists()
    {
        var fm36 = _context.Get<List<FM36Learner>>();
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        Assert.IsTrue(fm36.Any(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln));
    }
}
