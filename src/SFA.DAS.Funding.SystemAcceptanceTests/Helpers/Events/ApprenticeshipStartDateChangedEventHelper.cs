using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class ApprenticeshipStartDateChangedEventHelper
{
    internal static LearningStartDateChangedEvent CreateStartDateChangedMessageWithCustomValues(ScenarioContext context, DateTime actualStartDate, DateTime plannedEndDate, DateTime approvedDate)
    {
        var testData = context.Get<TestData>();
        var commitmentsEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        var learning = new LearningSqlClient().GetApprenticeship(testData.LearningKey);
        var episode = learning.Episodes.GetEpisode(commitmentsEvent.ProviderId, testData.LearningKey);
        var latestPrice = episode.Prices.OrderByDescending(p => p.StartDate).First();

        var fixture = new Fixture();
        return fixture.Build<LearningStartDateChangedEvent>()
            .With(_ => _.LearningKey, learning.Key)
            .With(_ => _.ApprovalsApprenticeshipId, episode.ApprovalsApprenticeshipId)
            .With(_ => _.StartDate, actualStartDate)
            .With(_ => _.ApprovedDate, approvedDate)
            .With(_ => _.Episode, new LearningEpisode
            {
                Prices = new List<LearningEpisodePrice>
                {
                    new LearningEpisodePrice
                    {
                        EndDate = plannedEndDate,
                        TrainingPrice = latestPrice.TrainingPrice,
                        EndPointAssessmentPrice = latestPrice.EndPointAssessmentPrice,
                        Key = Guid.NewGuid() ,
                        StartDate = actualStartDate,
                        TotalPrice = latestPrice.TotalPrice
                    },
                },
                EmployerAccountId = episode.EmployerAccountId,
                Ukprn = episode.Ukprn,
                Key = episode.Key,
                LegalEntityName = episode.LegalEntityName,
                TrainingCode = learning.TrainingCode,
            })
            .Create();
    }

    internal static async Task PublishApprenticeshipStartDateChangedEvent(LearningStartDateChangedEvent startDateChangedEvent)
    {
        await TestServiceBus.Das.SendStartDateChangedMessage(startDateChangedEvent);
    }
}
