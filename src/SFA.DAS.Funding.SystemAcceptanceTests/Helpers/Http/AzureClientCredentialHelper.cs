﻿using Azure.Core;
using Azure.Identity;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public interface IAzureClientCredentialHelper
{
    Task<string> GetAccessTokenAsync(string identifier);
}

public class AzureClientCredentialHelper : IAzureClientCredentialHelper
{
    private const int MaxRetries = 2;
    private readonly TimeSpan _networkTimeout = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(100);
    private readonly bool _isLocal;

    public AzureClientCredentialHelper()
    {
        var resourceEnvironmentName = Configurator.EnvironmentName;
        _isLocal = resourceEnvironmentName != null && resourceEnvironmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase);
    }

    public async Task<string> GetAccessTokenAsync(string identifier)
    {
        ChainedTokenCredential azureServiceTokenProvider;
        if (_isLocal)
        {
            azureServiceTokenProvider = new ChainedTokenCredential(
                new AzureCliCredential(options: new AzureCliCredentialOptions
                {
                    Retry = { NetworkTimeout = _networkTimeout, MaxRetries = MaxRetries, Delay = _delay, Mode = RetryMode.Fixed }
                }),
                new VisualStudioCredential(options: new VisualStudioCredentialOptions
                {
                    Retry = { NetworkTimeout = _networkTimeout, MaxRetries = MaxRetries, Delay = _delay, Mode = RetryMode.Fixed }
                }),
                new VisualStudioCodeCredential(options: new VisualStudioCodeCredentialOptions
                {
                    Retry = { NetworkTimeout = _networkTimeout, MaxRetries = MaxRetries, Delay = _delay, Mode = RetryMode.Fixed }
                }));

        }
        else
        {
            azureServiceTokenProvider = new ChainedTokenCredential(
                new ManagedIdentityCredential(options: new TokenCredentialOptions
                {
                    Retry = { NetworkTimeout = _networkTimeout, MaxRetries = MaxRetries, Delay = _delay, Mode = RetryMode.Fixed }
                }),
                new AzureCliCredential(options: new AzureCliCredentialOptions
                {
                    Retry = { NetworkTimeout = _networkTimeout, MaxRetries = MaxRetries, Delay = _delay, Mode = RetryMode.Fixed }
                }),
                new VisualStudioCredential(options: new VisualStudioCredentialOptions
                {
                    Retry = { NetworkTimeout = _networkTimeout, MaxRetries = MaxRetries, Delay = _delay, Mode = RetryMode.Fixed }
                }),
                new VisualStudioCodeCredential(options: new VisualStudioCodeCredentialOptions
                {
                    Retry = { NetworkTimeout = _networkTimeout, MaxRetries = MaxRetries, Delay = _delay, Mode = RetryMode.Fixed }
                }));
        }

        var accessToken = await azureServiceTokenProvider.GetTokenAsync(new TokenRequestContext(scopes: [identifier]));

        return accessToken.Token;
    }

}