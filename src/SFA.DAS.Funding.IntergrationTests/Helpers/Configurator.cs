using Microsoft.Extensions.Configuration;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.IntegrationTests.Helpers
{
    public static class Configurator
    {
        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true)
                .Build();
        }

        public static FundingConfig GetConfiguration()
        {
            var configuration = new FundingConfig();

            var iConfig = GetIConfigurationRoot();

            iConfig.Bind(configuration);

            return configuration;
        }

    }
}