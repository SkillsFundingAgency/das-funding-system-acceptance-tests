using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class PriceChangeApprovedEventHelper
{
    internal static LearningPriceChangedEvent CreatePriceChangeApprovedMessageWithCustomValues(this ScenarioContext context, decimal trainingPrice, decimal assessmentPrice, DateTime effectiveFromDate, DateTime approvedDate)
    {
        var testData = context.Get<TestData>();
        var commitmentsEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        var learning = new LearningSqlClient().GetApprenticeship(testData.LearningKey);
        var episode = learning.Episodes.GetEpisode(commitmentsEvent.ProviderId, commitmentsEvent.TrainingCode);
        var latestPrice = episode.Prices.OrderByDescending(p => p.StartDate).First();

        var fixture = new Fixture();
        return fixture.Build<LearningPriceChangedEvent>()
        .With(_ => _.LearningKey, learning.Key)
        .With(_ => _.ApprovalsApprenticeshipId, episode.ApprovalsApprenticeshipId)
        .With(_ => _.Episode, new LearningEpisode
        {
            Prices = new List<LearningEpisodePrice>()
            {
                    new LearningEpisodePrice
                    {
                        TrainingPrice = latestPrice.TrainingPrice,
                        EndPointAssessmentPrice = latestPrice.EndPointAssessmentPrice,
                        EndDate = effectiveFromDate.AddDays(-1),
                        Key = episode.Key,
                        StartDate = latestPrice.StartDate,
                        TotalPrice = latestPrice.TotalPrice,
                    },
                    {
                        new LearningEpisodePrice
                        {
                            TrainingPrice = trainingPrice,
                            EndPointAssessmentPrice = assessmentPrice,
                            EndDate = latestPrice.EndDate,
                            Key = Guid.NewGuid(),
                            StartDate = effectiveFromDate,
                            TotalPrice = trainingPrice + assessmentPrice
                        }
                    }
            },
            EmployerAccountId = episode.EmployerAccountId,
            Ukprn = episode.Ukprn,
            Key = episode.Key,
            LegalEntityName = episode.LegalEntityName,
            TrainingCode = episode.TrainingCode
        })
        .With(_ => _.EffectiveFromDate, effectiveFromDate)
        .With(_ => _.ApprovedDate, approvedDate)
        .Create();
    }

    internal static async Task PublishPriceChangeApprovedEvent(LearningPriceChangedEvent learningPriceChangedEvent)
    {
        await TestServiceBus.Das.SendPriceChangeApprovedMessage(learningPriceChangedEvent);
    }
}