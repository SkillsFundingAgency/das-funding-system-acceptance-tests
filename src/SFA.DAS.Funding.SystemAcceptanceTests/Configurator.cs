using Microsoft.Extensions.Configuration;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests
{
    public static class Configurator
    {
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
                .AddUserSecrets("FundingTestExecutionSecrets")
                .AddEnvironmentVariables()
                .Build();
        }

    }
}