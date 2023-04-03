
using CMT = SFA.DAS.CommitmentsV2.Messages.Events;
using APR = SFA.DAS.Apprenticeships.Types;
using AutoFixture;

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
                .With(_ => _.StartDate, new DateTime(actualStartDate.Year,actualStartDate.Month, 1))
                .With(_ => _.ActualStartDate, actualStartDate)
                .With(_ => _.EndDate, plannedEndDate)
                .With(_ => _.PriceEpisodes, new PriceEpisodeHelper().CreateSinglePriceEpisodeUsingStartDate(actualStartDate, agreedPrice))
                .With(_ => _.Uln, GenerateRandomUln())
                .With(_ => _.TrainingCode, trainingCode)
                .With(_ => _.DateOfBirth, DateTime.Now.AddYears((-18)))
                .With(_ => _.IsOnFlexiPaymentPilot, true)
                .Create();
        }

        public static int GenerateRandomNumberBetweenTwoValues(int min, int max) => new Random().Next(min, max);

        private string GenerateRandomUln()=> GenerateRandomNumberBetweenTwoValues(10, 99).ToString() + DateTime.Now.ToString("ssffffff");


        public CMT.ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithDoB(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, DateTime dob)
        {
            apprenticeshipCreatedEvent.DateOfBirth = dob;
            return apprenticeshipCreatedEvent;
        }

        public async Task PublishApprenticeshipApprovedMessage(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            await _context.Get<TestMessageBus>().Send(apprenticeshipCreatedEvent);

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

            _context.Set("Uln", apprenticeshipCreatedEvent.Uln);
            _context.Set("ApprenticeshipId", apprenticeshipCreatedEvent.ApprenticeshipId.ToString());
            _context.Set("ApprenticeshipKey", _context.Get<EarningsGeneratedEvent>().ApprenticeshipKey.ToString());
        }
    }
}