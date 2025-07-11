using System.Security.Claims;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public static class ClaimsHelper
{
    public static List<Claim> GetServiceAccountClaims()
    {
        var claims = new List<Claim>
        {
            CreateSubClaim(),
            CreateServiceAccountClaim(),
            CreateExpiryClaim()
        };

        return claims;
    }

    private static Claim CreateExpiryClaim()
    {
        long unixTime = ((DateTimeOffset)DateTime.UtcNow.AddMinutes(5)).ToUnixTimeSeconds();
        return new Claim("exp", unixTime.ToString());
    }

    private static Claim CreateSubClaim()
    {
        return new Claim("sub", "service-account-id");
    }

    private static Claim CreateServiceAccountClaim()
    {
        return new Claim("serviceAccount", "system-acceptance-tests");
    }
}