﻿namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

public class FundingConfig
{
    public string SharedServiceBusFqdn { get; set; } = "<not set>";
    public string NServiceBusLicense { get; set; } = "<not set>";
    public string LearningTransportStorageDirectory { get; set; } = "<not set>";
    public string EarningsEntityApi_BaseUrl { get; set; } = "<not set>";
    public string PaymentsEntityApi_BaseUrl { get; set; } = "<not set>";
    public string EarningsFunctionKey { get; set; } = "<not set>";
    public string PaymentsFunctionKey { get; set; } = "<not set>";
    public string ApprovalsEventHandlersQueue { get; set; } = "<not set>";
    public string FundingSystemAcceptanceTestQueue { get; set; } = "<not set>";
    public string ReleasePaymentsEventHandlersQueue { get; set; } = "<not set>";
}