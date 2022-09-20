using Microsoft.Extensions.Configuration;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests
{
    public static class Configurator
    {

        private readonly static string? EnvironmentName;
        private readonly static bool IsVstsExecution;
        private readonly static IConfigurationRoot _hostingConfig;

        static Configurator()
        {
            _hostingConfig = InitializeHostingConfig();
            EnvironmentName = GetEnvironmentName();
            IsVstsExecution = TestsExecutionInVsts();
        }

        private static IConfigurationRoot InitializeHostingConfig() => ConfigurationBuilder()
                .AddJsonFile("appsettings.ProjectConfig.json", true)
                .AddEnvironmentVariables()
                .Build();

        private static IConfigurationBuilder ConfigurationBuilder() => new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory());

        private static bool TestsExecutionInVsts() => !string.IsNullOrEmpty(GetAgentMachineName());

        private static string? GetAgentMachineName() => GetHostingConfigSection("ResourceEnvironmentName");

        private static string? GetEnvironmentName() => IsVstsExecution ? GetHostingConfigSection("ResourceEnvironmentName") :  GetHostingConfigSection("EnvironmentName");
        private static string? GetHostingConfigSection(string name) => _hostingConfig.GetSection(name)?.Value;

        public static FundingConfig GetConfiguration()
        {
            var configuration = new FundingConfig();

            var iConfig = GetIConfigurationRoot();

            iConfig.Bind(configuration);

            return configuration;
        }
        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.ProjectConfig.json", optional: true)
                .AddUserSecrets($"Funding_{EnvironmentName}_ExecutionSecrets")
                .AddEnvironmentVariables()
                .Build();
        }

    }
}