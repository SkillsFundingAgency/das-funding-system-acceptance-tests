﻿using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests
{
    public static class Configurator
    {

        public static readonly string? EnvironmentName;
        private static readonly bool IsVstsExecution;
        private static readonly IConfigurationRoot _hostingConfig;

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
        private static FundingConfig? _builtConfiguration;

        public static FundingConfig GetConfiguration()
        {
            if (_builtConfiguration == null)
            {
                _builtConfiguration = new FundingConfig();

                var iConfig = GetIConfigurationRoot();

                iConfig.Bind(_builtConfiguration);

                TestContext.Progress.WriteLine($"Available Env Variables: {JsonSerializer.Serialize(Environment.GetEnvironmentVariables())}");

                TestContext.Progress.WriteLine($"Built Configuration: {Environment.NewLine}{JsonSerializer.Serialize(_builtConfiguration)}");
            }

            return _builtConfiguration;
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