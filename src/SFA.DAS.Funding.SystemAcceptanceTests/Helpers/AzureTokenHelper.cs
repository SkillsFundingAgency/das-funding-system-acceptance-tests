using Azure.Core;
using Azure.Identity;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public class AzureTokenHelper
{
    private readonly TokenCredential _credential;

    public AzureTokenHelper()
    {
        _credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeInteractiveBrowserCredential = true
        });
    }

    public async Task<string> GetAccessTokenAsync(string scope)
    {
        var token = await _credential.GetTokenAsync(new TokenRequestContext(new[] { scope }));
        return token.Token;
    }
}