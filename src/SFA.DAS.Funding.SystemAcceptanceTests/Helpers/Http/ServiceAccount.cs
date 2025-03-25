using System.Text.Json.Serialization;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public class ServiceAccount
{
    [JsonPropertyName("sub")]
    public string? Sub { get; set; }
}