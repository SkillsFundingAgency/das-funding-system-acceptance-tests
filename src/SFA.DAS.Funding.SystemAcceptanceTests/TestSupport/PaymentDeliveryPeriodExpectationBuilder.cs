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

				result.Add(paymentDeliveryPeriodExpectation);
			}
		}

		return result;
	}
}