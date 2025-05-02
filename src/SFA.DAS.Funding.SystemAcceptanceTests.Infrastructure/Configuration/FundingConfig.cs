namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

public class FundingConfig
{
    public const string NotSet = "<not set>";

    public bool ShouldReleasePayments { get; set; } = true;
    public bool ShouldCleanUpTestRecords { get; set; } = false;
    public string SharedServiceBusFqdn { get; set; } = NotSet;
    public string SharedServiceBusTopicEndpoint { get; set; } = NotSet;
    public string LearningTransportStorageDirectory { get; set; } = NotSet;
    public string ApprenticeshipsDbConnectionString { get; set; } = NotSet;
    public string EarningsDbConnectionString { get; set; } = NotSet;
    public string PaymentsDbConnectionString { get; set; } = NotSet;
    public string WireMockBaseUrl { get; set; } = NotSet;
    public string ApprenticeshipAzureFunctionBaseUrl { get; set; } = NotSet;
    public string ApprenticeshipAzureFunctionKey { get; set; } = NotSet;
    public string EarningsOuterApiBaseUrl { get; set; } = NotSet;
    public string EarningsOuterSubscriptionKey { get; set; } = NotSet;
    public string EarningsFunctionKey { get; set; } = NotSet;
    public string PaymentsFunctionKey { get; set; } = NotSet;
    public string FundingSystemAcceptanceTestQueue { get; set; } = NotSet;
    public string FundingSystemAcceptanceTestSubscription { get; set; } = NotSet;
    public string PriceChangeApprovedEventHandlersQueue { get; set; } = NotSet;
    public string StartDateChangeApprovedEventHandlersQueue { get; set; } = NotSet;
    public string PaymentsFrozenEventHandlersQueue { get; set; } = NotSet;
    public string PaymentsUnfrozenEventHandlersQueue { get; set; } = NotSet;
    public string Pv2ServiceBusFqdn { get; set; } = NotSet;
    public string Pv2FundingSourceQueue { get; set; } = NotSet;
    public int EventWaitTimeInSeconds { get; set; } = 120;
    public string ApprovalCreatedQueue { get; set; } = NotSet;
    public string ApprenticeshipServiceBearerTokenSigningKey { get; set; } = NotSet;
    public string PaymentsFunctionsBaseUrl { get; set; } = NotSet;
    public string ApprenticeshipsInnerApiClientBaseUrl { get; set; } = NotSet;
    public string ApprenticeshipsInnerApiIdentifier { get; set; } = NotSet;
    public string EarningsInnerApiClientBaseUrl { get; set; } = NotSet;
    public string EarningsInnerApiScope { get; set; } = NotSet;
}