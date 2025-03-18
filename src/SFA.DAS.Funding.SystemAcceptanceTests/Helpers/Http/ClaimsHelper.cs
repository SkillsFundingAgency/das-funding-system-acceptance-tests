using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public static class ClaimsHelper
{
    public static List<Claim> GetServiceAccountClaims()
    {
        var claims = new List<Claim>
        {
            CreateSubClaim(),
            CreateExpiryClaim()
        };

        return claims;
    }

    private static Claim CreateExpiryClaim()
    {
        long unixTime = ((DateTimeOffset)DateTime.UtcNow.AddMinutes(15)).ToUnixTimeSeconds();
        return new Claim("exp", unixTime.ToString());
    }

    private static Claim CreateSubClaim()
    {
        return new Claim("sub", "service-account-id");
    }

    private static Claim CreateClaim<T>(this T user, Expression<Func<T, string?>> expression)
    {
        var value = expression.Compile().Invoke(user);
        return new Claim(GetJsonPropertyName<T, string>(expression), value ?? string.Empty);
    }

    internal static Claim CreateClaim<T>(this T user, Expression<Func<T, Organisation?>> expression)
    {
        var organisationValue = expression.Compile().Invoke(user);
        var organisationValueAsJson = JsonSerializer.Serialize(organisationValue);
        return new Claim(GetJsonPropertyName<T, Organisation?>(expression), organisationValueAsJson, JsonClaimValueTypes.Json);
    }

    private static string GetJsonPropertyName<T, TValueType>(Expression<Func<T, TValueType?>> expression)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new ArgumentException("Expression must be a member expression");
        }

        var objectPropertyName = memberExpression.Member.Name;

        var property = typeof(T).GetProperty(objectPropertyName);
        var attribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        return attribute?.Name;
    }

    public static List<Claim> GetProviderUserClaims()
    {
        var providerUser = new ProviderUser();

        var claims = new List<Claim>
        {
            providerUser.CreateClaim(x => x.Sub),
            providerUser.CreateClaim(x => x.Email),
            providerUser.CreateClaim(x => x.GivenName),
            providerUser.CreateClaim(x => x.FamilyName),
            providerUser.CreateClaim(x => x.Organisation),
            providerUser.CreateClaim(x => x.Sid),
            providerUser.CreateClaim(x => x.PortalComService),
            providerUser.CreateClaim(x => x.RoleCode),
            providerUser.CreateClaim(x => x.RoleId),
            providerUser.CreateClaim(x => x.RoleName),
            providerUser.CreateClaim(x => x.RoleNumericId),
            providerUser.CreateClaim(x => x.IdentityClaimsName),
            providerUser.CreateClaim(x => x.PortalComDisplayName),
            providerUser.CreateClaim(x => x.PortalComUkPrn),
            CreateExpiryClaim()
        };

        return claims;
    }
}

public class ProviderUser
{
    [JsonPropertyName("sub")]
    public string? Sub { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    [JsonPropertyName("organisation")]
    public Organisation? Organisation { get; set; }

    [JsonPropertyName("sid")]
    public string? Sid { get; set; }

    [JsonPropertyName("http://schemas.portal.com/service")]
    public string? PortalComService { get; set; }

    [JsonPropertyName("rolecode")]
    public string? RoleCode { get; set; }

    [JsonPropertyName("roleId")]
    public string? RoleId { get; set; }

    [JsonPropertyName("roleName")]
    public string? RoleName { get; set; }

    [JsonPropertyName("rolenumericid")]
    public string? RoleNumericId { get; set; }

    [JsonPropertyName("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")]
    public string? IdentityClaimsName { get; set; }

    [JsonPropertyName("http://schemas.portal.com/displayname")]
    public string? PortalComDisplayName { get; set; }

    [JsonPropertyName("http://schemas.portal.com/ukprn")]
    public string? PortalComUkPrn { get; set; }

    [JsonPropertyName("exp")]
    public int? Expiry { get; set; }
}

public class Organisation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}