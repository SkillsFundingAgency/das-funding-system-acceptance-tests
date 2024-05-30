namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public class PaymentExpectation
{
	//When adding a property to this class update the assertion helper extension code - PaymentDeliveryPeriodExpectationExtensions
	public decimal? Amount { get; set; }
	public bool? SentForPayment { get; set; }
	public Guid? EarningsProfileId { get; set; }
}