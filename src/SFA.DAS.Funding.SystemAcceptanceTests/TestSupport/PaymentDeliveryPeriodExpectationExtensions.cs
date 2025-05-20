using Newtonsoft.Json;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public static class PaymentDeliveryPeriodExpectationExtensions
{
	public static void AssertAgainstEntityArray(this IEnumerable<PaymentDeliveryPeriodExpectation> periodExpectations, List<Payments> paymentRecords)
	{
		foreach (var expectation in periodExpectations)
		{
			expectation.AssertAgainstEntityArray(paymentRecords);
		}
	}

	public static void AssertAgainstEventPayments(this IEnumerable<PaymentDeliveryPeriodExpectation> periodExpectations, IEnumerable<Payment> payments)
	{
		foreach (var expectation in periodExpectations)
		{
			expectation.AssertAgainstEventPayments(payments);
		}
	}

	public static void AssertAgainstEntityArray(this PaymentDeliveryPeriodExpectation periodExpectation, List<Payments> paymentRecords)
	{
		var periodPayments = paymentRecords.Where(x =>
			x.AcademicYear == periodExpectation.DeliveryPeriod.AcademicYear &&
			x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue);

		var errorMessage = $"Expected a payment on the ENTITY for delivery period {periodExpectation.DeliveryPeriod.AcademicYear}-{periodExpectation.DeliveryPeriod.PeriodValue}";

		Assert.That(periodPayments.Any(), $"{errorMessage}, but found no payments.");

		var filteredPeriodPayments = periodPayments;

		//Add more filter statements here as we add to PaymentExpectation class
		if (periodExpectation.Expectation.Amount != null)
		{
			errorMessage += $" with an Amount of {periodExpectation.Expectation.Amount},";
			filteredPeriodPayments = filteredPeriodPayments.Where(x => (decimal)x.Amount == periodExpectation.Expectation.Amount);
		}

		if (periodExpectation.Expectation.SentForPayment != null)
		{
			errorMessage += $" with a SentForPayment flag of {periodExpectation.Expectation.SentForPayment},";
			filteredPeriodPayments = filteredPeriodPayments.Where(x => x.SentForPayment == periodExpectation.Expectation.SentForPayment);
		}

		if (periodExpectation.Expectation.EarningsProfileId != null)
		{
			errorMessage += $" with an EarningsProfileId of {periodExpectation.Expectation.EarningsProfileId},";
			filteredPeriodPayments = filteredPeriodPayments.Where(x => x.EarningsProfileId == periodExpectation.Expectation.EarningsProfileId);
		}

            if (periodExpectation.Expectation.ProviderIncentiveAmount != null)
            {
                errorMessage += $" with an Amount of {periodExpectation.Expectation.ProviderIncentiveAmount},";
                filteredPeriodPayments = filteredPeriodPayments.Where(x => (decimal)x.Amount == periodExpectation.Expectation.ProviderIncentiveAmount && x.PaymentType == AdditionalPaymentType.ProviderIncentive.ToString());
            }

            if (periodExpectation.Expectation.EmployerIncentiveAmount != null)
            {
                errorMessage += $" with an Amount of {periodExpectation.Expectation.EmployerIncentiveAmount},";
                filteredPeriodPayments = filteredPeriodPayments.Where(x => (decimal)x.Amount == periodExpectation.Expectation.EmployerIncentiveAmount && x.PaymentType == AdditionalPaymentType.EmployerIncentive.ToString());
            }

            Assert.That(filteredPeriodPayments.Any(), $"{errorMessage} but only got the following payment(s): {JsonConvert.SerializeObject(periodPayments)}");
	}

	public static void AssertAgainstEventPayments(this PaymentDeliveryPeriodExpectation periodExpectation, IEnumerable<Payment> payments)
	{
		var periodPayments = payments.Where(x =>
			x.AcademicYear == periodExpectation.DeliveryPeriod.AcademicYear &&
			x.DeliveryPeriod == periodExpectation.DeliveryPeriod.PeriodValue);

		var errorMessage = $"Expected a payment on the EVENT for delivery period {periodExpectation.DeliveryPeriod.AcademicYear}-{periodExpectation.DeliveryPeriod.PeriodValue}";

		Assert.That(periodPayments.Any(), $"{errorMessage}, but found no payments.");

		var filteredPeriodPayments = periodPayments;

		//Add more filter statements here as we add to PaymentExpectation class
		//Note some properties not on the event so not asserted here
		if (periodExpectation.Expectation.Amount != null)
		{
			errorMessage += $" with an Amount of {periodExpectation.Expectation.Amount},";
			filteredPeriodPayments = filteredPeriodPayments.Where(x => (decimal)x.Amount == periodExpectation.Expectation.Amount);
		}

            if (periodExpectation.Expectation.ProviderIncentiveAmount != null)
            {
                errorMessage += $" with a ProviderIncentiveAmount of {periodExpectation.Expectation.ProviderIncentiveAmount},";
                filteredPeriodPayments = filteredPeriodPayments.Where(x => (decimal)x.Amount == periodExpectation.Expectation.ProviderIncentiveAmount && x.PaymentType == AdditionalPaymentType.ProviderIncentive.ToString());
            }

            if (periodExpectation.Expectation.EmployerIncentiveAmount != null)
            {
                errorMessage += $" with a EmployerIncentiveAmount of {periodExpectation.Expectation.EmployerIncentiveAmount},";
                filteredPeriodPayments = filteredPeriodPayments.Where(x => (decimal)x.Amount == periodExpectation.Expectation.EmployerIncentiveAmount && x.PaymentType == AdditionalPaymentType.EmployerIncentive.ToString());
            }

            Assert.That(filteredPeriodPayments.Any(), $"{errorMessage} but only got the following payment(s): {JsonConvert.SerializeObject(periodPayments)}");
	}
}
