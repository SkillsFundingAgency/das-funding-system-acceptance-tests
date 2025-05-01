using Azure.Core;
using Azure.Identity;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public class AzureTokenHelper
{
    public async Task<string> GetAccessTokenAsync(string scope)
    {
        var credential = new AzureCliCredential();
        var token = await credential.GetTokenAsync(new TokenRequestContext([scope]));
        return token.Token;
    }
}