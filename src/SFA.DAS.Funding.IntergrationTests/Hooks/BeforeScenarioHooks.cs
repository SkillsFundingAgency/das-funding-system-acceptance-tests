using SFA.DAS.Funding.IntegrationTests.Helpers;
using SFA.DAS.Funding.IntegrationTests.Infrastructure;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.AzureDurableFunctions;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.IntegrationTests.Hooks
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
            context.Set(new FundingOrchestrationHelper(config));
            context.Set(new Helper(context));
        }

        private static void PrintConfig(FundingConfig config)
        {
            Console.WriteLine($"[CONFIG] NServiceBusConnectionString:{config.NServiceBusConnectionString}");
            Console.WriteLine($"[CONFIG] NServiceBusLicense:{config.NServiceBusLicense}");
            Console.WriteLine($"[CONFIG] LearningTransportStorageDirectory:{config.LearningTransportStorageDirectory}");
        }
    }
}