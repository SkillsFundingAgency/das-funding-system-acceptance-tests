namespace SFA.DAS.Funding.IntegrationTests.Infrastructure.Configuration
{
    public class FundingConfig
    {
        public string NServiceBusConnectionString { get; set; } = "<not set>";
        public string NServiceBusLicense { get; set; } = "<not set>";
        public string LearningTransportStorageDirectory { get; set; } = "<not set>";
        public string FunctionsBaseUrl { get; set; } = "<not set>";
        public string FunctionsAuthenticationCode { get; set; } = "<not set>";
    }
}
