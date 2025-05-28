using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class ApprenticeshipStartDateChangedEventHelper
{
    internal static ApprenticeshipStartDateChangedEvent CreateStartDateChangedMessageWithCustomValues(ScenarioContext context, DateTime actualStartDate, DateTime plannedEndDate, DateTime approvedDate)
    {
        var testData = context.Get<TestData>();
        var apprenticeshipCreatedEvent = testData.ApprenticeshipCreatedEvent;

        var fixture = new Fixture();
        return fixture.Build<ApprenticeshipStartDateChangedEvent>()
            .With(_ => _.ApprenticeshipKey, apprenticeshipCreatedEvent.ApprenticeshipKey)
            .With(_ => _.ApprenticeshipId, apprenticeshipCreatedEvent.ApprovalsApprenticeshipId)
            .With(_ => _.StartDate, actualStartDate)
            .With(_ => _.ApprovedDate, approvedDate)
            .With(_ => _.Episode, new ApprenticeshipEpisode
            {
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new ApprenticeshipEpisodePrice
                    {
                        EndDate = plannedEndDate,
                        FundingBandMaximum = apprenticeshipCreatedEvent.Episode.Prices.First().FundingBandMaximum,
                        TrainingPrice = apprenticeshipCreatedEvent.Episode.Prices.First().TrainingPrice,
                        EndPointAssessmentPrice = apprenticeshipCreatedEvent.Episode.Prices.First().EndPointAssessmentPrice,
                        Key = Guid.NewGuid() ,
                        StartDate = actualStartDate,
                        TotalPrice = apprenticeshipCreatedEvent.Episode.Prices.First().TotalPrice
                    },
                },
                EmployerAccountId = apprenticeshipCreatedEvent.Episode.EmployerAccountId,
                Ukprn = apprenticeshipCreatedEvent.Episode.Ukprn,
                Key = apprenticeshipCreatedEvent.Episode.Key,
                LegalEntityName = apprenticeshipCreatedEvent.Episode.LegalEntityName,
                TrainingCode = apprenticeshipCreatedEvent.Episode.TrainingCode,
            })
            .Create();
    }

    internal static async Task PublishApprenticeshipStartDateChangedEvent(ApprenticeshipStartDateChangedEvent startDateChangedEvent)
    {
        await TestServiceBus.Das.SendStartDateChangedMessage(startDateChangedEvent);
    }
}
