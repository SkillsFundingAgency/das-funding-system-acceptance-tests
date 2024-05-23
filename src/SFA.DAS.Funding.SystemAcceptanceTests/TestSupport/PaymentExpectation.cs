namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public class PaymentExpectation
{
	public decimal? Amount { get; set; }
	public bool? SentForPayment { get; set; }
	public Guid? EarningsProfileId { get; set; }
}