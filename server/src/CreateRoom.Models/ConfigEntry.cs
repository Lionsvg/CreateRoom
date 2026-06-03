using System.Text.Json.Serialization;

namespace CreateRoom.Models;

/// <summary>
/// One entry of the global config returned by GET /api/config/v2.
/// Matches the real response shape: [{ "Key": "...", "Value": "...", ... }, ...]
/// </summary>
public class ConfigEntry
{
    [JsonPropertyName("Key")] public string Key { get; set; } = "";
    [JsonPropertyName("Value")] public string? Value { get; set; }
    [JsonPropertyName("ActiveExperiments")] public object? ActiveExperiments { get; set; }
    [JsonPropertyName("StartTime")] public string? StartTime { get; set; }
    [JsonPropertyName("EndTime")] public string? EndTime { get; set; }
}
