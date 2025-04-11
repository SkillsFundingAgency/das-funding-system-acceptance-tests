using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http
{
    public static class BearerTokenHelper
    {
        public static string AddClaimsToBearerToken(string token, Dictionary<string, string> newClaims, string signingKey)
        {
            var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.ToList();
            claims.AddRange(newClaims.Select(newClaim => new Claim(newClaim.Key, newClaim.Value)));

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var result = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials,
                expires: DateTime.UtcNow.AddSeconds(60)
            );

            return new JwtSecurityTokenHandler().WriteToken(result);
        }
    }
}
