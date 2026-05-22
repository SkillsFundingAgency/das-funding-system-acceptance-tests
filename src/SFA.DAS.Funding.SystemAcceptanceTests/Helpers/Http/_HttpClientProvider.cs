using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;


/// <summary>
/// In the absence of DI this is used to prevent multiple instances of the same ApiClient being created
/// </summary>
internal static class HttpClientProvider
{
    private static readonly ConcurrentDictionary<string, HttpClient> _apiClients = new();

    internal static HttpClient GetClient(string baseUrl, SecureGatewayConfig? secureUrl = null)
    {
        return _apiClients.GetOrAdd(baseUrl, url =>
        {
            var client = CreateHttpClient(baseUrl, secureUrl);
            return client;
        });
    }

    private static HttpClient CreateHttpClient(string url, SecureGatewayConfig? secureConfig = null)
    {
        if(secureConfig == null || !secureConfig.UseSecureGateway)
        {
            return new HttpClient { BaseAddress = new Uri(url) };
        }

        var credential = new DefaultAzureCredential();
        var secretClient = new SecretClient(new Uri(secureConfig.SecretClientUrl), credential);

        var secret = secretClient.GetSecret(secureConfig.SecretName);

        if (!secret.HasValue)
        {
            throw new Exception($"Has errored - {secret.GetRawResponse().Content.ToDynamicFromJson()}");
        }

        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(new X509Certificate2(Convert.FromBase64String(secret.Value.Value)));

        var client = new HttpClient(handler) { BaseAddress = new Uri(secureConfig.SecureGatewayBaseUrl) };

        return client;
    }
}