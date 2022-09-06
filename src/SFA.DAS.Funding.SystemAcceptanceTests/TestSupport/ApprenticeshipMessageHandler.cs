
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    internal class ApprenticeshipMessageHandler
    {
        private readonly ScenarioContext _context;

        public ApprenticeshipMessageHandler(ScenarioContext context)
        {
            _context = context;
        }

        public ApprenticeshipCreatedEvent CreateApprenticeshipCreatedMessageWithCustomValues(DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice, string uln)
        {
            var fixture = new Fixture();
             return fixture.Build<ApprenticeshipCreatedEvent>()
                .With(_ => _.StartDate, actualStartDate)
                .With(_ => _.EndDate, plannedEndDate)
                .With(_ => _.PriceEpisodes, new PriceEpisodeHelper().CreateSinglePriceEpisodeUsingStartDate(actualStartDate, agreedPrice))
                //.With(_ => _.FundingBandMaximum, agreedPrice)
                .With(_ => _.Uln, uln)
                .Create();
        }

        public ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithFundingBandMaximumValue(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, decimal fundingBandMax)
        {
           // apprenticeshipCreatedEvent.FundingBandMaximum = fundingBandMax;

            return apprenticeshipCreatedEvent;
        }

        public async Task PublishApprenticeshipApprovedMessage(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            await _context.Get<TestMessageBus>().Publish(apprenticeshipCreatedEvent);

            await WaitHelper.WaitForIt(() => EarningsGeneratedEventHandler.ReceivedEvents.Where(x => x.FundingPeriods.Any (y => y.Uln.ToString() == apprenticeshipCreatedEvent.Uln)).Any(), "Failed to find published event");

            //EarningsGeneratedEvent earningsGeneratedEvent = EarningsGeneratedEventHandler.ReceivedEvents.First();

            //FundingPeriod fundingPeriod = earningsGeneratedEvent.FundingPeriods.First();


           // await WaitHelper.WaitForIt(() => fundingPeriod.Uln.ToString() == apprenticeshipCreatedEvent.Uln, "Failed to find published event");
        }

        public EarningsGeneratedEvent ReadEarningsGeneratedMessage(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            // return EarningsGeneratedEventHandler.ReceivedEvents.Where(x => x.FundingPeriods.First().Uln == apprenticeshipCreatedEvent.Uln).First();
            return new EarningsGeneratedEvent ();
        }


    }
}