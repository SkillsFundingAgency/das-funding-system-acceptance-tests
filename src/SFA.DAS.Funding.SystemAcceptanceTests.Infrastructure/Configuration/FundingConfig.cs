﻿namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

public class FundingConfig
{
    public string SharedServiceBusFqdn { get; set; } = "<not set>";
    public string NServiceBusLicense { get; set; } = "<not set>";
    public string LearningTransportStorageDirectory { get; set; } = "<not set>";
    public string FunctionsBaseUrl { get; set; } = "<not set>";
    public string FunctionsAuthenticationCode { get; set; } = "<not set>";
}