using System.Text.Json.Serialization;

namespace CreateRoom.Models;

/// <summary>Minimal account model for GET /Accounts/account/me (placeholder shape).</summary>
public class Account
{
    [JsonPropertyName("accountId")] public long AccountId { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; } = "";
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = "";
}
