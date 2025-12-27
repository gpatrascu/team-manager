using Newtonsoft.Json;

namespace api.Domain.Entities;

public class Team
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("admins")]
    public List<string> Admins { get; set; } = new(); // googleId list

    [JsonProperty("inviteToken")]
    public string? InviteToken { get; set; }

    [JsonProperty("inviteTokenExpiry")]
    public DateTime? InviteTokenExpiry { get; set; }

    [JsonProperty("members")]
    public List<TeamMember> Members { get; set; } = new();

    [JsonProperty("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Domain methods
    public bool IsUserAdmin(string userId) => Admins.Contains(userId);

    public bool IsTokenValid() => InviteTokenExpiry == null || InviteTokenExpiry > DateTime.UtcNow;

    public TeamMember? GetMember(string memberId) => Members.FirstOrDefault(m => m.Id == memberId);

    public bool HasMember(string userId) => Members.Any(m => m.UserId == userId);

    public IEnumerable<TeamMember> GetPendingMembers() => Members.Where(m => m.IsPending());

    public IEnumerable<TeamMember> GetActiveMembers() => Members.Where(m => m.IsActive());
}
