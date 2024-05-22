namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
	public class PaymentPeriodExpectation
	{
		public byte DeliveryPeriod { get; set; }
		public double Amount { get; set; }
		public bool SentForPayment { get; set; }
		public Guid EarningsProfileId { get; set; }
	}
}
