using Newtonsoft.Json;

namespace api.Domain.Entities;

public class TeamMember
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("userId")]
    public string? UserId { get; set; } // googleId from Azure SWA

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonProperty("role")]
    public string Role { get; set; } = "Member";

    [JsonProperty("status")]
    public string Status { get; set; } = "pending"; // pending, active, inactive

    [JsonProperty("joinedAt")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("invitedBy")]
    public string? InvitedBy { get; set; }

    [JsonProperty("approvedBy")]
    public string? ApprovedBy { get; set; }

    [JsonProperty("approvedAt")]
    public DateTime? ApprovedAt { get; set; }

    // Domain methods
    public void Approve(string approverUserId)
    {
        Status = "active";
        ApprovedBy = approverUserId;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = "inactive";
    }

    public bool IsPending() => Status == "pending";

    public bool IsActive() => Status == "active";

    public bool IsInactive() => Status == "inactive";
}
