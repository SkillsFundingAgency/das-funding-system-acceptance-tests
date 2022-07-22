namespace SFA.DAS.Funding.IntegrationTests.Infrastructure.Configuration
{
    public class FundingConfig
    {
        public string NServiceBusConnectionString { get; set; } = "UseLearningEndpoint=true";
       // public string NServiceBusConnectionString { get; set; } = "das-at-shared-ns.servicebus.windows.net";
        public string LearningTransportStorageDirectory { get; set; } = "C:\\temp\\LearningTransport";
        public string FunctionsBaseUrl { get; set; }
        public string FunctionsAuthenticationCode { get; set; }
    }
}
