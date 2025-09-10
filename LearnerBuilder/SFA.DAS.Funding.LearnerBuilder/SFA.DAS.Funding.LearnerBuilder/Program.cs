using SFA.DAS.Funding.LearnerBuilder;
using SFA.DAS.Funding.SystemAcceptanceTests;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.MessageBus;

class Program
{
    static async Task Main(string[] args)
    {
        var config = Configurator.GetConfiguration();

        Console.WriteLine("======================================================================");
        Console.WriteLine("                       Learner Builder");
        Console.WriteLine($"                  Environment: {config.EnvironmentName}");
        Console.WriteLine("======================================================================");
        Console.WriteLine("               This will take a minute to initialize");

        // Deactive console output during initialization
        var originalOut = Console.Out;
        Console.SetOut(TextWriter.Null);

        var messageBus = await GetServiceBus();
        var mainLoop = new MainLoop(config, messageBus);

        // Restore console output
        Console.SetOut(originalOut);
        Console.WriteLine("               Initializing complete");

        await mainLoop.Run();

        await TearDownServiceBusConnection();
    }

    private static async Task<TestMessageBus> GetServiceBus()
    {
        try
        {
            await TestRunHooks.SetUpAzureServiceBusSubscription();
            TestRunHooks.StartEndpoints();
            return TestServiceBus.Das;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not connect to Service Bus:");
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private static async Task TearDownServiceBusConnection()
    {
        await TestRunHooks.TearDownSubscription();
        TestRunHooks.StopEndpoints();
    }
}