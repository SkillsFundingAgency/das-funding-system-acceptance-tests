using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using APR = SFA.DAS.Apprenticeships.Types;
using CMT = SFA.DAS.CommitmentsV2.Messages.Events;

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
                .With(_ => _.ApprenticeshipEmployerTypeOnApproval, ApprenticeshipEmployerType.Levy)
                .With(_ => _.AccountId, 3871)
                .With(_ => _.TransferSenderId, (long?)null)
                .With(_ => _.DateOfBirth, DateTime.Now.AddYears((-18)))
                .With(_ => _.IsOnFlexiPaymentPilot, true)
                .With(x => x.TrainingCourseVersion, "1.0")
                .Create();
        }

        public static int GenerateRandomNumberBetweenTwoValues(int min, int max) => new Random().Next(min, max);

        private string GenerateRandomUln()=> GenerateRandomNumberBetweenTwoValues(10, 99).ToString() + DateTime.Now.ToString("ssffffff");


        public CMT.ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithDoB(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, DateTime dob)
        {
            apprenticeshipCreatedEvent.DateOfBirth = dob;
            return apprenticeshipCreatedEvent;
        }

        public CMT.ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithEmployerType(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, ApprenticeshipEmployerType employerType)
        {
            apprenticeshipCreatedEvent.ApprenticeshipEmployerTypeOnApproval = employerType;
            return apprenticeshipCreatedEvent;
        }

        public async Task PublishApprenticeshipApprovedMessage(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            await TestServiceBus.Das.SendApprenticeshipApprovedMessage(apprenticeshipCreatedEvent);

            await WaitHelper.WaitForIt(() =>
            {
                APR.ApprenticeshipCreatedEvent? apprenticeshipEvent =
                    ApprenticeshipsTypesEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.Uln == apprenticeshipCreatedEvent.Uln).message;
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
                    EarningsGeneratedEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.Uln == apprenticeshipCreatedEvent.Uln).message;
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