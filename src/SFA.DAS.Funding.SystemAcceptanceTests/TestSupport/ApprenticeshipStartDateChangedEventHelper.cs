﻿using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

internal class ApprenticeshipStartDateChangedEventHelper
{
    private readonly ScenarioContext _context;

    public ApprenticeshipStartDateChangedEventHelper(ScenarioContext context)
    {
        _context = context;
    }
    public ApprenticeshipStartDateChangedEvent CreateStartDateChangedMessageWithCustomValues(DateTime actualStartDate, DateTime plannedEndDate, DateTime approvedDate)
    {
        var apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();

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

    public async Task PublishApprenticeshipStartDateChangedEvent(ApprenticeshipStartDateChangedEvent startDateChangedEvent)
    {
        await TestServiceBus.Das.SendStartDateChangedMessage(startDateChangedEvent);
    }
}