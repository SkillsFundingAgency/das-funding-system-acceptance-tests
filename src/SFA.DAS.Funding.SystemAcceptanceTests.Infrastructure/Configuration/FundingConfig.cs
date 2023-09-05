namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

public class FundingConfig
{
    public string SharedServiceBusFqdn { get; set; } = "<not set>";
    public string NServiceBusLicense { get; set; } = "<not set>";
    public string SharedServiceBusTopicEndpoint { get; set; } = "<not set>";
    public string LearningTransportStorageDirectory { get; set; } = "<not set>";
    public string EarningsEntityApi_BaseUrl { get; set; } = "<not set>";
    public string PaymentsEntityApi_BaseUrl { get; set; } = "<not set>";
    public string EarningsFunctionKey { get; set; } = "<not set>";
    public string PaymentsFunctionKey { get; set; } = "<not set>";
    public string FundingSystemAcceptanceTestQueue { get; set; } = "<not set>";
    public string ApprovalsEventHandlersQueue { get; set; } = "<not set>";
    public string FundingSystemAcceptanceTestSubscription { get; set; } = "<not set>";
    public string ReleasePaymentsEventHandlersQueue { get; set; } = "<not set>";
    public string Pv2ServiceBusFqdn { get; set; } = "<not set>";
    public string Pv2FundingSourceQueue { get; set; } = "<not set>";
    public string EventTypes { get; set; } = "<not set>";
}