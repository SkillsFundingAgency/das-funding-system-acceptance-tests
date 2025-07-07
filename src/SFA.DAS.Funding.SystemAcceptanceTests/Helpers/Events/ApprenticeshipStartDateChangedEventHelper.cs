using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class ApprenticeshipStartDateChangedEventHelper
{
    internal static LearningStartDateChangedEvent CreateStartDateChangedMessageWithCustomValues(ScenarioContext context, DateTime actualStartDate, DateTime plannedEndDate, DateTime approvedDate)
    {
        var testData = context.Get<TestData>();
        var apprenticeshipCreatedEvent = testData.LearningCreatedEvent;

        var fixture = new Fixture();
        return fixture.Build<LearningStartDateChangedEvent>()
            .With(_ => _.LearningKey, apprenticeshipCreatedEvent.LearningKey)
            .With(_ => _.ApprovalsApprenticeshipId, apprenticeshipCreatedEvent.ApprovalsApprenticeshipId)
            .With(_ => _.StartDate, actualStartDate)
            .With(_ => _.ApprovedDate, approvedDate)
            .With(_ => _.Episode, new LearningEpisode
            {
                Prices = new List<LearningEpisodePrice>
                {
                    new LearningEpisodePrice
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

    internal static async Task PublishApprenticeshipStartDateChangedEvent(LearningStartDateChangedEvent startDateChangedEvent)
    {
        await TestServiceBus.Das.SendStartDateChangedMessage(startDateChangedEvent);
    }
}
