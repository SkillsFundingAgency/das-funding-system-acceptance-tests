namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

#pragma warning disable CS8618 
public class SecureGatewayConfig
{
    public bool UseSecureGateway { get; set; } = false;
    public string SecureGatewayBaseUrl { get; set; }
    /// <summary>
    /// Keyvault containing certificate
    /// </summary>
    public string SecretClientUrl { get; set; }
    /// <summary>
    /// Keyvault secret name containing the client certificate
    /// </summary>
    public string SecretName { get; set; }
}

public static class SecureGatewayConfigExtensions
{
    public static SecureGatewayConfig GetSecureGatewayConfig(this FundingConfig config, Func<FundingConfig, string?> gatewayUrl)
    {
        var gatewayConfig = new SecureGatewayConfig();

        var url = gatewayUrl(config);

        if (string.IsNullOrEmpty(url) || url == FundingConfig.NotSet) 
        {
            return gatewayConfig;
        }

        gatewayConfig.UseSecureGateway = true;
        gatewayConfig.SecureGatewayBaseUrl = url;
        gatewayConfig.SecretClientUrl = config.CertificateSecretClientUrl;
        gatewayConfig.SecretName = config.CertificateSecretName;
        return gatewayConfig;
    }
}
#pragma warning restore CS8618