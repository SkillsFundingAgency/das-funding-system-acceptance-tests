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
                .With(_ => _.TrainingCourseVersion, "1.0")
                .With(_ => _.ProviderId, 88888888)
                .Create();
        }

        private static String GenerateRandomUln()
        {
            String randomUln = GenerateRandomNumberBetweenTwoValues(10, 99).ToString()
                + DateTime.Now.ToString("ssffffff");

            for (int i = 1; i < 30; i++)
            {
                if (IsValidCheckSum(randomUln))
                {
                    return randomUln;
                }
                randomUln = (long.Parse(randomUln) + 1).ToString();
            }
            throw new Exception("Unable to generate ULN");
        }

        private static int GenerateRandomNumberBetweenTwoValues(int min, int max) => new Random().Next(min, max);

        private static bool IsValidCheckSum(string uln)
        {
            var ulnCheckArray = uln.ToCharArray()
                                    .Select(c => long.Parse(c.ToString()))
                                    .ToList();

            var multiplier = 10;
            long checkSumValue = 0;
            for (var i = 0; i < 10; i++)
            {
                checkSumValue += ulnCheckArray[i] * multiplier;
                multiplier--;
            }

            return checkSumValue % 11 == 10;
        }


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