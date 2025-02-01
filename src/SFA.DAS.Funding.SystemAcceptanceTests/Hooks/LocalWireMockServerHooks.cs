using WireMock.Server;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

[Binding]
public static class LocalWireMockServerHooks
{
    private static WireMockServer? _wireMockServer;

    [BeforeTestRun]
    public static void StartLocalWireMockServer()
    {
        if (Configurator.EnvironmentName == "LOCAL")
        {
            var localWireMockUrl = new Uri(Configurator.GetConfiguration().WireMockBaseUrl);
            if (localWireMockUrl.Host != "localhost")
                throw new ArgumentException("WireMockBaseUrl must be localhost when running tests against local environment.");

            _wireMockServer = WireMockServer.Start(localWireMockUrl.Port);
        }
    }

    [AfterTestRun]
    public static void StopLocalWireMockServer()
    {
        _wireMockServer?.Stop();
    }
}