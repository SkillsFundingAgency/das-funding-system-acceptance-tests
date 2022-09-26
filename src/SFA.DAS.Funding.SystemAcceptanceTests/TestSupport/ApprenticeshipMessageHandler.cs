
using CMT = SFA.DAS.CommitmentsV2.Messages.Events;
using APR = SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    internal class ApprenticeshipMessageHandler
    {
        private readonly ScenarioContext _context;
        

        public ApprenticeshipMessageHandler(ScenarioContext context)
        {
            _context = context;
        }

        public CMT.ApprenticeshipCreatedEvent CreateApprenticeshipCreatedMessageWithCustomValues(DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode)
        {
            var fixture = new Fixture();
             return fixture.Build<CMT.ApprenticeshipCreatedEvent>()
                .With(_ => _.StartDate, actualStartDate)
                .With(_ => _.EndDate, plannedEndDate)
                .With(_ => _.PriceEpisodes, new PriceEpisodeHelper().CreateSinglePriceEpisodeUsingStartDate(actualStartDate, agreedPrice))
                .With(_ => _.Uln, fixture.Create<long>().ToString)
                .With(_ => _.TrainingCode, trainingCode)
                .With(_ => _.DateOfBirth, DateTime.Now.AddYears((-18)))
                .Create();
        }
        
        public CMT.ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithDoB(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, DateTime dob)
        {
            apprenticeshipCreatedEvent.DateOfBirth = dob;
            return apprenticeshipCreatedEvent;
        }

        public async Task PublishApprenticeshipApprovedMessage(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            await _context.Get<TestMessageBus>().Publish(apprenticeshipCreatedEvent);
            
            await WaitHelper.WaitForIt(() =>
            {
                CMT.ApprenticeshipCreatedEvent? commitmentEvent = 
                    CommitmentsEventHandler.ReceivedEvents.FirstOrDefault(x => x.Uln == apprenticeshipCreatedEvent.Uln);
                if (commitmentEvent != null)
                {
                    _context.Set(commitmentEvent);
                    return true;
                }
                return false;
            },"Failed to find published event in Commitments");
            
            await WaitHelper.WaitForIt(() =>
            {
                APR.ApprenticeshipCreatedEvent? apprenticeshipEvent =
                    ApprenticeshipsTypesEventHandler.ReceivedEvents.FirstOrDefault(x => x.Uln == apprenticeshipCreatedEvent.Uln);
                if (apprenticeshipEvent != null)
                {
                    _context.Set(apprenticeshipEvent);
                    return true;
                }
                return false;
            }, "Failed to find published event in apprenticeships");
            
            await WaitHelper.WaitForIt(() =>
            {
                EarningsGeneratedEvent? earningsEvent = 
                    EarningsGeneratedEventHandler.ReceivedEvents.FirstOrDefault(x =>
                    x.FundingPeriods.Any(y => y.Uln.ToString() == apprenticeshipCreatedEvent.Uln));
                if (earningsEvent != null)
                {
                    _context.Set(earningsEvent);
                    return true;
                }
                return false;
            }, "Failed to find published event in Earnings");
        }
    }
}