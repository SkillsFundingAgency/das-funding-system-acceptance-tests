using AutoFixture;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using PriceChangeApprovedEvent = SFA.DAS.Apprenticeships.Types.PriceChangeApprovedEvent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class PriceChangeMessageHandler
    {
        private readonly ScenarioContext _context;
        private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;

        public PriceChangeMessageHandler(ScenarioContext context)
        {
            _context = context;
        }

        public PriceChangeApprovedEvent CreatePriceChangeApprovedMessageWithCustomValues(decimal trainingPrice, decimal assessmentPrice, DateTime effectiveFromDate, DateTime approvedDate)
        {
            _apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();

            var fixture = new Fixture();
            return fixture.Build<PriceChangeApprovedEvent>()
            .With(_ => _.ApprenticeshipKey, _apprenticeshipCreatedEvent.ApprenticeshipKey)
            .With(_ => _.ApprenticeshipId, _apprenticeshipCreatedEvent.ApprovalsApprenticeshipId)
            .With(_ => _.TrainingPrice, trainingPrice)
            .With(_ => _.AssessmentPrice, assessmentPrice)
            .With(_ => _.EffectiveFromDate, effectiveFromDate)
            .With(_ => _.ApprovedDate, approvedDate)
            .With(_ => _.EmployerAccountId, _apprenticeshipCreatedEvent.EmployerAccountId)
            .With(_ => _.ProviderId, _apprenticeshipCreatedEvent.UKPRN)
            .Create();
        }

        public async Task PublishPriceChangeApprovedEvent(PriceChangeApprovedEvent priceChangeApprovedEvent)
        {
            await TestServiceBus.Das.SendPriceChangeApprovedMessage(priceChangeApprovedEvent);
        }

        public async Task ReceiveEarningsRecalculatedEvent(Guid apprenticeshipKey)
        {
            await WaitHelper.WaitForIt(() =>
            {
                ApprenticeshipEarningsRecalculatedEvent? earningsRecalculatedEvent =
                    ApprenticeshipEarningsRecalculatedEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.ApprenticeshipKey == apprenticeshipKey).message;

                if (earningsRecalculatedEvent != null)
                {
                    _context.Set(earningsRecalculatedEvent);
                    return true;
                }
                return false;
            }, "Failed to find published Apprenticeship Earnings Recalculated Event");
        }
    }
}
