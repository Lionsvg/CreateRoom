using System.Text.Json.Serialization;

namespace CreateRoom.Models;

/// <summary>
/// The version-check response — tells the client it's up to date so it proceeds past the boot gate.
/// REAL schema decoded from the IL2CPP metadata of RecNet.Runtime.dll's VersionCheckResponse:
///   VersionStatus           : enum  0=ValidForPlay, 1=UpdateRequired        (field @ obj+0x10)
///   UpdateNotificationStage : enum  0=None,1=Silent,2=Warn,3=Prompt,4=Require (field @ obj+0x14)
///   IsCrossPlayDisabled     : bool
///   RequiresUpdate          : computed = (VersionStatus==1) || (UpdateNotificationStage==4)
/// The blocking orange "update required" screen shows when VersionStatus==1 OR UpdateNotificationStage==4,
/// so returning 0/0 = "all good, play allowed". RecNet uses PascalCase JSON (confirmed from the config response).
/// </summary>
public class VersionCheck
{
    [JsonPropertyName("VersionStatus")] public int VersionStatus { get; set; } = 0;                       // ValidForPlay
    [JsonPropertyName("UpdateNotificationStage")] public int UpdateNotificationStage { get; set; } = 0;    // None
    [JsonPropertyName("IsCrossPlayDisabled")] public bool IsCrossPlayDisabled { get; set; } = false;
    [JsonPropertyName("RequiresUpdate")] public bool RequiresUpdate { get; set; } = false;
}

/// <summary>A single GameConfig entry (RecNet.GameConfig { Key, Value, ActiveExperiments, StartTime, EndTime }).</summary>
public class GameConfig
{
    [JsonPropertyName("Key")] public string Key { get; set; } = "";
    [JsonPropertyName("Value")] public string Value { get; set; } = "";
    [JsonPropertyName("ActiveExperiments")] public string[]? ActiveExperiments { get; set; }
    [JsonPropertyName("StartTime")] public string? StartTime { get; set; }
    [JsonPropertyName("EndTime")] public string? EndTime { get; set; }
}

/// <summary>GET /Matchmaking/player — the player's matchmaking/session record.</summary>
public class MatchmakingPlayer
{
    [JsonPropertyName("playerId")] public long PlayerId { get; set; }
    [JsonPropertyName("accountId")] public long AccountId { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; } = "";
    [JsonPropertyName("isOnline")] public bool IsOnline { get; set; } = true;
    [JsonPropertyName("statusVisibility")] public int StatusVisibility { get; set; } = 1;
}

/// <summary>GET /Room_server/photon_access_token — token the client uses to connect to Photon realtime.</summary>
public class PhotonAccessToken
{
    [JsonPropertyName("Token")] public string Token { get; set; } = "";
    [JsonPropertyName("AppId")] public string AppId { get; set; } = "00000000-0000-0000-0000-000000000000";
    [JsonPropertyName("Region")] public string Region { get; set; } = "us";
    [JsonPropertyName("ExpirationDate")] public string ExpirationDate { get; set; } = "2099-01-01T00:00:00Z";
}

/// <summary>A room (GET /Room_server/rooms/{id}, /featuredrooms/current, etc.) — minimal shape.</summary>
public class Room
{
    [JsonPropertyName("RoomId")] public long RoomId { get; set; }
    [JsonPropertyName("Name")] public string Name { get; set; } = "";
    [JsonPropertyName("Description")] public string Description { get; set; } = "";
    [JsonPropertyName("CreatorAccountId")] public long CreatorAccountId { get; set; }
    [JsonPropertyName("MaxPlayerCalculationMode")] public int MaxPlayerCalculationMode { get; set; }
    [JsonPropertyName("CloningPermission")] public int CloningPermission { get; set; }
}
