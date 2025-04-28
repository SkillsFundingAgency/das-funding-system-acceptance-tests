using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public static class PaymentDeliveryPeriodExpectationBuilder
{
	/// <summary>
	/// This builds a list of expectations between the two given delivery periods (inclusive), all with the same given expectation
	/// </summary>
	/// <param name="firstDeliveryPeriod">The first delivery period to include</param>
	/// <param name="lastDeliveryPeriod">The last delivery period to include</param>
	/// <param name="expectation">The expectation to use for all the delivery periods</param>
	/// <returns>A list of expectations</returns>
	public static List<PaymentDeliveryPeriodExpectation> BuildForDeliveryPeriodRange(Period firstDeliveryPeriod, Period lastDeliveryPeriod, PaymentExpectation expectation)
	{
		var result = new List<PaymentDeliveryPeriodExpectation>();

		var startYear = firstDeliveryPeriod.AcademicYear.GetStartingYearFromAcademicYear();
		var endYear = lastDeliveryPeriod.AcademicYear.GetStartingYearFromAcademicYear();

		for (int year = startYear; year <= endYear; year++)
		{
			byte startPeriod = (year == startYear) ? firstDeliveryPeriod.PeriodValue : (byte)1;
			byte endPeriod = (year == endYear) ? lastDeliveryPeriod.PeriodValue : (byte)12;

			for (byte period = startPeriod; period <= endPeriod; period++)
			{
				var paymentDeliveryPeriodExpectation = new PaymentDeliveryPeriodExpectation
				{
					DeliveryPeriod = new Period(year.GetAcademicYearFromStartingYear(), period),
					Expectation = expectation
				};

				if(paymentDeliveryPeriodExpectation.Expectation.Amount != 0)
				{
                    result.Add(paymentDeliveryPeriodExpectation);
                }
				
			}
		}

		return result;
	}

    /// <summary>
    /// This builds an expectation for an incentive payment.
    /// </summary>
    /// <param name="startDate">The start date of the apprenticeship</param>
    /// <param name="incentiveNumber">The incentive number (1 or 2) to build the expectation for</param>
    /// <param name="paymentType">The payment type to build the expectation for (ProviderIncentive or EmployerIncentive)</param>
    /// <returns>An incentive expectation</returns>
    public static PaymentDeliveryPeriodExpectation BuildForIncentive(DateTime startDate, byte incentiveNumber, AdditionalPaymentType paymentType)
    {
        if (incentiveNumber is not (1 or 2))
            throw new Exception("PaymentDeliveryPeriodExpectationBuilder only supports building payments with an incentive number of 1 or 2");

        var expectedPeriod = incentiveNumber == 2
            ? startDate.AddDays(364).ToAcademicYearAndPeriod() // 365th day of learning
            : startDate.AddDays(89).ToAcademicYearAndPeriod(); // 90th day of learning

        return new PaymentDeliveryPeriodExpectation
        {
            DeliveryPeriod = new Period(expectedPeriod.AcademicYear, expectedPeriod.Period),
            Expectation = new PaymentExpectation
            {
                ProviderIncentiveAmount = paymentType == AdditionalPaymentType.ProviderIncentive ? 500 : null,
                EmployerIncentiveAmount = paymentType == AdditionalPaymentType.EmployerIncentive ? 500 : null
            }
        };
    }
}