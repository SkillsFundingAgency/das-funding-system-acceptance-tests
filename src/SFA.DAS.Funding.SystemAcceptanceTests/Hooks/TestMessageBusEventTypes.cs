using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

public static class TestMessageBusEventTypes
{
    public static Type[] Das = new Type[] { typeof(ApprenticeshipCreatedEvent), typeof(EarningsGeneratedEvent), typeof(FinalisedOnProgammeLearningPaymentEvent), typeof(PaymentsGeneratedEvent) };
    public static Type[] Pv2 = new Type[] { typeof(CalculatedRequiredLevyAmount) };
}