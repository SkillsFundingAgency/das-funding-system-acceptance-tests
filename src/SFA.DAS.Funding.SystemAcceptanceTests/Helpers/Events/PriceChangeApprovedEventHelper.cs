using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class PriceChangeApprovedEventHelper
{
    internal static ApprenticeshipPriceChangedEvent CreatePriceChangeApprovedMessageWithCustomValues(this ScenarioContext context, decimal trainingPrice, decimal assessmentPrice, DateTime effectiveFromDate, DateTime approvedDate)
    {
        var apprenticeshipCreatedEvent = context.Get<ApprenticeshipCreatedEvent>();

        var fixture = new Fixture();
        return fixture.Build<ApprenticeshipPriceChangedEvent>()
        .With(_ => _.ApprenticeshipKey, apprenticeshipCreatedEvent.ApprenticeshipKey)
        .With(_ => _.ApprenticeshipId, apprenticeshipCreatedEvent.ApprovalsApprenticeshipId)
        .With(_ => _.Episode, new ApprenticeshipEpisode
        {
            Prices = new List<ApprenticeshipEpisodePrice>()
            {
                    new ApprenticeshipEpisodePrice
                    {
                        TrainingPrice = apprenticeshipCreatedEvent.Episode.Prices[0].TrainingPrice,
                        EndPointAssessmentPrice = apprenticeshipCreatedEvent.Episode.Prices[0].EndPointAssessmentPrice,
                        EndDate = effectiveFromDate.AddDays(-1),
                        FundingBandMaximum = apprenticeshipCreatedEvent.Episode.Prices[0].FundingBandMaximum,
                        Key = apprenticeshipCreatedEvent.Episode.Key,
                        StartDate = apprenticeshipCreatedEvent.Episode.Prices[0].StartDate,
                        TotalPrice = apprenticeshipCreatedEvent.Episode.Prices[0].TotalPrice,
                    },
                    {
                        new ApprenticeshipEpisodePrice
                        {
                            TrainingPrice = trainingPrice,
                            EndPointAssessmentPrice = assessmentPrice,
                            EndDate = apprenticeshipCreatedEvent.Episode.Prices[0].EndDate,
                            FundingBandMaximum = apprenticeshipCreatedEvent.Episode.Prices[0].FundingBandMaximum,
                            Key = Guid.NewGuid(),
                            StartDate = effectiveFromDate,
                            TotalPrice = trainingPrice + assessmentPrice
                        }
                    }
            },
            EmployerAccountId = apprenticeshipCreatedEvent.Episode.EmployerAccountId,
            Ukprn = apprenticeshipCreatedEvent.Episode.Ukprn,
            Key = apprenticeshipCreatedEvent.Episode.Key,
            LegalEntityName = apprenticeshipCreatedEvent.Episode.LegalEntityName,
            TrainingCode = apprenticeshipCreatedEvent.Episode.TrainingCode
        })
        .With(_ => _.EffectiveFromDate, effectiveFromDate)
        .With(_ => _.ApprovedDate, approvedDate)
        .Create();
    }

    internal static async Task PublishPriceChangeApprovedEvent(ApprenticeshipPriceChangedEvent apprenticeshipPriceChangedEvent)
    {
        await TestServiceBus.Das.SendPriceChangeApprovedMessage(apprenticeshipPriceChangedEvent);
    }
}