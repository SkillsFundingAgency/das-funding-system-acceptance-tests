using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks
{
    [Binding]
    public class BeforeScenarioHooks
    {
        [BeforeScenario(Order = 1)]
        public void SetUpHelpers(ScenarioContext context)
        {
            var config = Configurator.GetConfiguration();
            PrintConfig(config); // TODO: DELETE!!!
            context.Set(config);
        }

        private static void PrintConfig(FundingConfig config)
        {
            Console.WriteLine($"[CONFIG] SharedServiceBusFqdn:{config.SharedServiceBusFqdn}");
            Console.WriteLine($"[CONFIG] NServiceBusLicense:{config.NServiceBusLicense}");
            Console.WriteLine($"[CONFIG] LearningTransportStorageDirectory:{config.LearningTransportStorageDirectory}");
        }
    }
}