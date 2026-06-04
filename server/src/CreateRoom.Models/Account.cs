using System.Text.Json.Serialization;

namespace CreateRoom.Models;

/// <summary>
/// Account shape for GET /Accounts/account/me and /account/bulk.
/// Field names recovered from the modern client's RecNet.Account / RecNet.SelfAccount DTOs
/// (PersonalPronouns, IdentityFlags, CreatedAt, IsJunior, IsMetaPlatformBlocked) + standard RecRoom fields.
/// </summary>
public class Account
{
    [JsonPropertyName("accountId")] public long AccountId { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; } = "";
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = "";
    [JsonPropertyName("profileImage")] public string? ProfileImage { get; set; }
    [JsonPropertyName("bannerImage")] public string? BannerImage { get; set; }
    [JsonPropertyName("isJunior")] public bool IsJunior { get; set; }
    [JsonPropertyName("isMetaPlatformBlocked")] public bool IsMetaPlatformBlocked { get; set; }
    [JsonPropertyName("personalPronouns")] public int PersonalPronouns { get; set; }
    [JsonPropertyName("identityFlags")] public int IdentityFlags { get; set; }
    [JsonPropertyName("platforms")] public int Platforms { get; set; } = 1;
    [JsonPropertyName("createdAt")] public string CreatedAt { get; set; } = "2017-01-01T00:00:00Z";
}

/// <summary>Self-account adds private fields (RecNet.SelfAccount): Birthday, JuniorState, AvailableUsernameChanges.</summary>
public class SelfAccount : Account
{
    [JsonPropertyName("birthday")] public string Birthday { get; set; } = "2000-01-01T00:00:00Z";
    [JsonPropertyName("juniorState")] public int JuniorState { get; set; }
    [JsonPropertyName("availableUsernameChanges")] public int AvailableUsernameChanges { get; set; } = 1;
}
