namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.AzureDurableFunctions
{
    public class OrchestratorStatusResponse
    {
        public string RuntimeStatus { get; set; }
        public string CustomStatus { get; set; }
        public string Output { get; set; }
    }
}
