using System.Text.Json.Serialization;

namespace CreateRoom.Models;

/// <summary>Standard OAuth token response for POST /Auth/connect/token.</summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
    [JsonPropertyName("token_type")] public string TokenType { get; set; } = "Bearer";
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; } = 3600;
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }
    [JsonPropertyName("scope")] public string? Scope { get; set; }
}
