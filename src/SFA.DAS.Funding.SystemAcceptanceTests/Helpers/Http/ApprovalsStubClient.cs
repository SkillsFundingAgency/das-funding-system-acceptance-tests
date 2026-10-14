using Newtonsoft.Json;
using System.Globalization;
using System.Net;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

/// <summary>
/// das-api-stub stand-in for Approvals' PUT approvals/{learningKey}
/// LearnerData outer API calls this when Learning reports a change that needs approval.
/// </summary>
internal class ApprovalsStubClient
{
    private const string ApprovalsVerb = "Put";
    private const string AutoApprovedStatus = "autoApproved";
    private const string StartDateChangeType = "StartDate";

    private readonly WireMockClient _wireMockClient = new();
    private readonly HashSet<Guid> _learningKeysSetUp = [];

    public IReadOnlyCollection<Guid> LearningKeysSetUp => _learningKeysSetUp;

    /// <summary>
    /// The next start date change for this learning is auto-approved. Approvals echoes the old and new values back,
    /// and the outer API copies them into the ApprovedLearningUpdatedEvent it publishes.
    /// </summary>
    public async Task SetUpAutoApproval(Guid learningKey, DateTime oldStartDate, DateTime newStartDate)
    {
        var response = new ApprovalsResponse
        {
            Changes =
            [
                new ApprovalsChange
                {
                    ChangeType = StartDateChangeType,
                    ApprovalStatus = AutoApprovedStatus,
                    Data = new ApprovalsChangeData
                    {
                        Old = ToWireDate(oldStartDate),
                        New = ToWireDate(newStartDate)
                    }
                }
            ]
        };

        await _wireMockClient.CreateMockResponse(ApprovalsUrl(learningKey), response, ApprovalsVerb, HttpStatusCode.Created);
        _learningKeysSetUp.Add(learningKey);
    }

    public async Task RemoveApprovalSetUp(Guid learningKey)
    {
        await _wireMockClient.DeleteMockResponse(ApprovalsUrl(learningKey), ApprovalsVerb);
        _learningKeysSetUp.Remove(learningKey);
    }

    private static string ApprovalsUrl(Guid learningKey) => $"approvals/{learningKey}";

    private static string ToWireDate(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

#pragma warning disable CS8618 // Null properties

    public class ApprovalsResponse
    {
        [JsonProperty("changes")]
        public List<ApprovalsChange> Changes { get; set; } = [];

        [JsonProperty("prices")]
        public List<object> Prices { get; set; } = [];
    }

    public class ApprovalsChange
    {
        [JsonProperty("changeType")]
        public string ChangeType { get; set; }

        [JsonProperty("approvalStatus")]
        public string ApprovalStatus { get; set; }

        [JsonProperty("data")]
        public ApprovalsChangeData Data { get; set; }
    }

    public class ApprovalsChangeData
    {
        [JsonProperty("old")]
        public string Old { get; set; }

        [JsonProperty("new")]
        public string New { get; set; }
    }
#pragma warning restore CS8618
}
