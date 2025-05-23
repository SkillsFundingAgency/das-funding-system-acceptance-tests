﻿using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.CommitmentsV2.Types;
using APR = SFA.DAS.Apprenticeships.Types;
using CMT = SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;

internal static class ApprenticeshipEventHelper
{
    internal static CMT.ApprenticeshipCreatedEvent CreateApprenticeshipCreatedMessageWithCustomValues(DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode)
    {
        var fixture = new Fixture();
        return fixture.Build<CMT.ApprenticeshipCreatedEvent>()
           .With(_ => _.StartDate, new DateTime(actualStartDate.Year, actualStartDate.Month, 1))
           .With(_ => _.ActualStartDate, actualStartDate)
           .With(_ => _.EndDate, plannedEndDate)
           .With(_ => _.PriceEpisodes, new PriceEpisodeHelper().CreateSinglePriceEpisodeUsingStartDate(actualStartDate, agreedPrice))
           .With(_ => _.Uln, GenerateRandomUln())
           .With(_ => _.TrainingCode, trainingCode)
           .With(_ => _.ApprenticeshipEmployerTypeOnApproval, ApprenticeshipEmployerType.Levy)
           .With(_ => _.AccountId, 3871)
           .With(_ => _.TransferSenderId, (long?)null)
           .With(_ => _.DateOfBirth, DateTime.Now.AddYears((-25)))
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


    internal static CMT.ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithDoB(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, DateTime dob)
    {
        apprenticeshipCreatedEvent.DateOfBirth = dob;
        return apprenticeshipCreatedEvent;
    }

    internal static CMT.ApprenticeshipCreatedEvent UpdateApprenticeshipCreatedMessageWithEmployerType(CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent, ApprenticeshipEmployerType employerType)
    {
        apprenticeshipCreatedEvent.ApprenticeshipEmployerTypeOnApproval = employerType;
        return apprenticeshipCreatedEvent;
    }

    internal static async Task PublishApprenticeshipApprovedMessage(this ScenarioContext context, CMT.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        await TestServiceBus.Das.SendApprenticeshipApprovedMessage(apprenticeshipCreatedEvent);

        await WaitHelper.WaitForIt(() =>
        {
            APR.ApprenticeshipCreatedEvent? apprenticeshipEvent =
                ApprenticeshipsTypesEventHandler.ReceivedEvents.FirstOrDefault(x => x.message.Uln == apprenticeshipCreatedEvent.Uln).message;
            if (apprenticeshipEvent != null)
            {
                context.Set(apprenticeshipEvent);
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
                context.Set(earningsEvent);
                return true;
            }
            return false;
        }, "Failed to find published event in Earnings");


        context.Set("Uln", apprenticeshipCreatedEvent.Uln);
        context.Set("ApprenticeshipId", apprenticeshipCreatedEvent.ApprenticeshipId.ToString());
        context.Set("ApprenticeshipKey", context.Get<EarningsGeneratedEvent>().ApprenticeshipKey.ToString());

    }
}