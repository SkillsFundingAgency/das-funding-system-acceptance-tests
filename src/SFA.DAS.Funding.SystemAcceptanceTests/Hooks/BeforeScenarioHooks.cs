namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks
{
    [Binding]
    public class BeforeScenarioHooks
    {
        [BeforeScenario(Order = 1)]
        public void SetUpHelpers(ScenarioContext context)
        {
            var config = Configurator.GetConfiguration();
            context.Set(config);
            Console.WriteLine($"Begin Scenario {context.ScenarioInfo.Title}");
        }
    }
}