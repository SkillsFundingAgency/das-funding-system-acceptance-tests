namespace SFA.DAS.Funding.IntegrationTests.Infrastructure.AzureDurableFunctions
{
    public class OrchestratorStatusResponse
    {
        public string RuntimeStatus { get; set; }
        public string CustomStatus { get; set; }
        public string Output { get; set; }
    }
}
