using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class PriceChangeApprovedEventHelper
    {
        private readonly ScenarioContext _context;
        private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;

        public PriceChangeApprovedEventHelper(ScenarioContext context)
        {
            _context = context;
        }

        public ApprenticeshipPriceChangedEvent CreatePriceChangeApprovedMessageWithCustomValues(decimal trainingPrice, decimal assessmentPrice, DateTime effectiveFromDate, DateTime approvedDate)
        {
            _apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();

            var fixture = new Fixture();
            return fixture.Build<ApprenticeshipPriceChangedEvent>()
            .With(_ => _.ApprenticeshipKey, _apprenticeshipCreatedEvent.ApprenticeshipKey)
            .With(_ => _.ApprenticeshipId, _apprenticeshipCreatedEvent.ApprovalsApprenticeshipId)
            .With(_ => _.Episode, new ApprenticeshipEpisode
            {
                Prices = new List<ApprenticeshipEpisodePrice>()
                {
                    new ApprenticeshipEpisodePrice
                    {
                        TrainingPrice = _apprenticeshipCreatedEvent.Episode.Prices[0].TrainingPrice,
                        EndPointAssessmentPrice = _apprenticeshipCreatedEvent.Episode.Prices[0].EndPointAssessmentPrice,
                        EndDate = effectiveFromDate.AddDays(-1),
                        FundingBandMaximum = _apprenticeshipCreatedEvent.Episode.Prices[0].FundingBandMaximum,
                        Key = _apprenticeshipCreatedEvent.Episode.Key,
                        StartDate = _apprenticeshipCreatedEvent.Episode.Prices[0].StartDate,
                        TotalPrice = _apprenticeshipCreatedEvent.Episode.Prices[0].TotalPrice,
                    },
                    {
                        new ApprenticeshipEpisodePrice
                        {
                            TrainingPrice = trainingPrice,
                            EndPointAssessmentPrice = assessmentPrice,
                            EndDate = _apprenticeshipCreatedEvent.Episode.Prices[0].EndDate,
                            FundingBandMaximum = _apprenticeshipCreatedEvent.Episode.Prices[0].FundingBandMaximum,
                            Key = Guid.NewGuid(),
                            StartDate = effectiveFromDate,
                            TotalPrice = trainingPrice + assessmentPrice
                        }
                    }
                },
                EmployerAccountId = _apprenticeshipCreatedEvent.Episode.EmployerAccountId,
                Ukprn = _apprenticeshipCreatedEvent.Episode.Ukprn,
                Key = _apprenticeshipCreatedEvent.Episode.Key,
                LegalEntityName = _apprenticeshipCreatedEvent.Episode.LegalEntityName,
                TrainingCode = _apprenticeshipCreatedEvent.Episode.TrainingCode
            })
            .With(_ => _.EffectiveFromDate, effectiveFromDate)
            .With(_ => _.ApprovedDate, approvedDate)
            .Create();
        }

        public async Task PublishPriceChangeApprovedEvent(ApprenticeshipPriceChangedEvent apprenticeshipPriceChangedEvent)
        {
            await TestServiceBus.Das.SendPriceChangeApprovedMessage(apprenticeshipPriceChangedEvent);
        }
    }
}
