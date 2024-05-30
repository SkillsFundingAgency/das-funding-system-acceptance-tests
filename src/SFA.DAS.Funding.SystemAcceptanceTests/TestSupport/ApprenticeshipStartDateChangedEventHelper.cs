using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

internal class ApprenticeshipStartDateChangedEventHelper
{
    private readonly ScenarioContext _context;

    public ApprenticeshipStartDateChangedEventHelper(ScenarioContext context)
    {
        _context = context;
    }

    public ApprenticeshipStartDateChangedEvent CreateStartDateChangedMessageWithCustomValues(DateTime actualStartDate, DateTime approvedDate)
    {
        var apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();

        var fixture = new Fixture();
        return fixture.Build<ApprenticeshipStartDateChangedEvent>()
            .With(_ => _.ApprenticeshipKey, apprenticeshipCreatedEvent.ApprenticeshipKey)
            .With(_ => _.ApprenticeshipId, apprenticeshipCreatedEvent.ApprovalsApprenticeshipId)
            .With(_ => _.ActualStartDate, actualStartDate)
            .With(_ => _.ApprovedDate, approvedDate)
            .With(_ => _.EmployerAccountId, apprenticeshipCreatedEvent.EmployerAccountId)
            .With(_ => _.ProviderId, apprenticeshipCreatedEvent.UKPRN)
            .Create();
    }

    public async Task PublishApprenticeshipStartDateChangedEvent(ApprenticeshipStartDateChangedEvent startDateChangedEvent)
    {
        await TestServiceBus.Das.SendStartDateChangedMessage(startDateChangedEvent);
    }
}