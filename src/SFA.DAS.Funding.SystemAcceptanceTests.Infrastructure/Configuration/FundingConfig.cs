﻿namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

public class FundingConfig
{
    public string SharedServiceBusFqdn { get; set; } = "<not set>";
    public string SharedServiceBusTopicEndpoint { get; set; } = "<not set>";
    public string LearningTransportStorageDirectory { get; set; } = "<not set>";
    public string EarningsEntityApi_BaseUrl { get; set; } = "<not set>";
    public string EarningsDbConnectionString { get; set; } = "<not set>";
    public string PaymentsEntityApi_BaseUrl { get; set; } = "<not set>";
    public string EarningsFunctionKey { get; set; } = "<not set>";
    public string PaymentsFunctionKey { get; set; } = "<not set>";
    public string FundingSystemAcceptanceTestQueue { get; set; } = "<not set>";
    public string ApprovalsEventHandlersQueue { get; set; } = "<not set>";
    public string FundingSystemAcceptanceTestSubscription { get; set; } = "<not set>";
    public string ReleasePaymentsEventHandlersQueue { get; set; } = "<not set>";
    public string PriceChangeApprovedEventHandlersQueue { get; set; } = "<not set>";
    public string StartDateChangeApprovedEventHandlersQueue { get; set; } = "<not set>";
    public string PaymentsFrozenEventHandlersQueue { get; set; } = "<not set>";
    public string PaymentsUnfrozenEventHandlersQueue { get; set; } = "<not set>";
    public string Pv2ServiceBusFqdn { get; set; } = "<not set>";
    public string Pv2FundingSourceQueue { get; set; } = "<not set>";
    public int EventWaitTimeInSeconds { get; set; } = 120;
}