namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

public class FundingConfig
{
    public bool ShouldReleasePayments { get; set; } = true;
    public bool ShouldCleanUpTestRecords { get; set; } = false;
    public string SharedServiceBusFqdn { get; set; } = "<not set>";
    public string SharedServiceBusTopicEndpoint { get; set; } = "<not set>";
    public string LearningTransportStorageDirectory { get; set; } = "<not set>";
    public string ApprenticeshipsDbConnectionString { get; set; } = "<not set>";
    public string EarningsDbConnectionString { get; set; } = "<not set>";
    public string PaymentsDbConnectionString { get; set; } = "<not set>";
    public string WireMockBaseUrl { get; set; } = "<not set>";
    public string ApprenticeshipAzureFunctionBaseUrl { get; set; } = "<not set>";
    public string ApprenticeshipAzureFunctionKey { get; set; } = "<not set>";
    public string EarningsOuterApiBaseUrl { get; set; } = "<not set>";
    public string EarningsOuterSubscriptionKey { get; set; } = "<not set>";
    public string EarningsFunctionKey { get; set; } = "<not set>";
    public string PaymentsFunctionKey { get; set; } = "<not set>";
    public string FundingSystemAcceptanceTestQueue { get; set; } = "<not set>";
    public string ApprovalsEventHandlersQueue { get; set; } = "<not set>";
    public string FundingSystemAcceptanceTestSubscription { get; set; } = "<not set>";
    public string PriceChangeApprovedEventHandlersQueue { get; set; } = "<not set>";
    public string StartDateChangeApprovedEventHandlersQueue { get; set; } = "<not set>";
    public string PaymentsFrozenEventHandlersQueue { get; set; } = "<not set>";
    public string PaymentsUnfrozenEventHandlersQueue { get; set; } = "<not set>";
    public string Pv2ServiceBusFqdn { get; set; } = "<not set>";
    public string Pv2FundingSourceQueue { get; set; } = "<not set>";
    public int EventWaitTimeInSeconds { get; set; } = 120;
    public string ApprovalCreatedQueue { get; set; } = "<not set>";
    public string ApprenticeshipServiceBearerTokenSigningKey { get; set; } = "<not set>";
    public string PaymentsFunctionsBaseUrl { get; set; } = "<not set>";
}