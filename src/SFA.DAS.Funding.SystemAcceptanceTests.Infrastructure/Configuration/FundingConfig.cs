namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

public class FundingConfig
{
    public string SharedServiceBusFqdn { get; set; } = "<not set>";
    public string TenantId { get; set; } = "<not set>";
    public string AppRegistrationClientId { get; set; } = "<not set>";
    public string AppRegistrationClientSecret { get; set; } = "<not set>";
    public string NServiceBusLicense { get; set; } = "<not set>";
    public string LearningTransportStorageDirectory { get; set; } = "<not set>";
    public string FunctionsBaseUrl { get; set; } = "<not set>";
    public string FunctionsAuthenticationCode { get; set; } = "<not set>";
}