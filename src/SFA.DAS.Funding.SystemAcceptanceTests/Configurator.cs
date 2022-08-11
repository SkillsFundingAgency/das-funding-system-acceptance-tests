using Microsoft.Extensions.Configuration;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests
{
    public static class Configurator
    {
        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Directory.GetCurrentDirectory())
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