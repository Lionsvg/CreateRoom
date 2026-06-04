using CreateRoom.Models;
using CreateRoom.Api;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Debug logging for Kestrel so we SEE incoming connections + TLS handshake failures (diagnosing the game's redirected connection).
builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Debug);

// Ports: HTTP for easy curl testing; HTTPS for the real client (set to 80/443 + run as admin when
// testing against the game). Override with CREATEROOM_HTTP_PORT / CREATEROOM_HTTPS_PORT.
int httpPort  = int.TryParse(Environment.GetEnvironmentVariable("CREATEROOM_HTTP_PORT"),  out var hp) ? hp : 8080;
int httpsPort = int.TryParse(Environment.GetEnvironmentVariable("CREATEROOM_HTTPS_PORT"), out var sp) ? sp : 8443;
string certDir = Path.Combine(builder.Environment.ContentRootPath, "certs");
string certPem = Path.Combine(certDir, "cert.pem"), keyPem = Path.Combine(certDir, "key.pem");
bool https = File.Exists(certPem) && File.Exists(keyPem);

// Capture log: every HTTP request + every TLS ClientHello (with SNI) -> a file we can read,
// so we see exactly what the client does — and whether pinning cuts it before any request. Fresh per run.
string logPath = Environment.GetEnvironmentVariable("CREATEROOM_LOG")
    ?? Path.Combine(builder.Environment.ContentRootPath, "test-capture.log");
var logLock = new object();
Action<string> Log = msg =>
{
    var line = $"{DateTime.UtcNow:HH:mm:ss.fff}  {msg}";
    Console.WriteLine(line);
    try { lock (logLock) File.AppendAllText(logPath, line + Environment.NewLine); } catch { }
};
try { File.WriteAllText(logPath, $"=== CreateRoom capture {DateTime.UtcNow:u} ==={Environment.NewLine}"); } catch { }

builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenAnyIP(httpPort);
    if (https)
        try
        {
            // On Windows, schannel can't use the ephemeral private key from CreateFromPemFile for SERVER TLS
            // (handshake silently drops -> "Cipher is (NONE)"). Round-trip through PFX to persist the key so schannel accepts it.
            using var ephemeral = X509Certificate2.CreateFromPemFile(certPem, keyPem);
            var cert = new X509Certificate2(ephemeral.Export(X509ContentType.Pkcs12));
            string caPem = Path.Combine(certDir, "ca-cert.pem");
            var chain = new X509Certificate2Collection();
            if (File.Exists(caPem)) chain.Add(X509Certificate2.CreateFromPem(File.ReadAllText(caPem)));
            o.ListenAnyIP(httpsPort, l => l.UseHttps(h =>
            {
                h.ServerCertificateSelector = (connCtx, sni) =>
                    { Log($"[TLS] ClientHello  SNI={sni ?? "(none)"}  from {connCtx?.RemoteEndPoint}"); return cert; };
                if (chain.Count > 0) h.ServerCertificateChain = chain;          // send leaf + CreateRoom Dev CA (full chain)
                h.SslProtocols = System.Security.Authentication.SslProtocols.Tls12; // match the game's BouncyCastle (TLS 1.2 only)
            }));
            Log($"[https] cert: {cert.Subject}  chain certs: {chain.Count}  (TLS 1.2 forced)");
        }
        catch (Exception e) { Console.WriteLine("[warn] HTTPS disabled: " + e.Message); https = false; }
});

var app = builder.Build();

// Log every request — this is how we learn what the client actually asks for.
app.Use(async (ctx, next) =>
{
    Log($"{ctx.Request.Method,-5} {ctx.Request.Scheme}://{ctx.Request.Host}{ctx.Request.Path}{ctx.Request.QueryString}  UA=\"{ctx.Request.Headers.UserAgent}\"");
    await next();
});

// The dev player identity served to the client.
const long Acct = 2; const string User = "LocalPlayer"; const string Display = "Local Player";
SelfAccount Me() => new() { AccountId = Acct, Username = User, DisplayName = Display, IsJunior = false };
MatchmakingPlayer Mp() => new() { PlayerId = Acct, AccountId = Acct, Username = User };

// === In-memory world: coherent data so listings and lookups agree (a real mini-backend, not random stubs) ===
var dorm   = new Room { RoomId = 1,  Name = "Dorm",            Description = "Your dorm",        CreatorAccountId = Acct };
var room10 = new Room { RoomId = 10, Name = "CreateRoom Plaza", Description = "Community hub",     CreatorAccountId = Acct };
var room11 = new Room { RoomId = 11, Name = "The Hangout",      Description = "Chill spot",        CreatorAccountId = Acct };
var allRooms = new List<Room> { dorm, room10, room11 };
var featured = new List<Room> { room10, room11 };
// look a room up by id; fall back to a generated one so unknown ids still resolve coherently
Room RoomById(long id) { var r = allRooms.Find(x => x.RoomId == id); return r ?? new Room { RoomId = id, Name = "Room " + id, Description = "Room " + id, CreatorAccountId = Acct }; }

// === OIDC discovery + JWKS (client validates OAuth tokens against these) ===
app.MapGet("/.well-known/openid-configuration", () => Results.Json(TokenService.Discovery()));
app.MapGet("/Auth/.well-known/openid-configuration", () => Results.Json(TokenService.Discovery()));
app.MapGet("/Auth/.well-known/openid-configuration/jwks", () => Results.Json(TokenService.Jwks()));
app.MapGet("/.well-known/openid-configuration/jwks", () => Results.Json(TokenService.Jwks()));

// === Auth ===
app.MapPost("/Auth/connect/token", () => Results.Json(new TokenResponse
{
    AccessToken = TokenService.IssueAccessToken(Acct, User),
    TokenType = "Bearer",
    ExpiresIn = 3600,
    RefreshToken = "createroom.refresh." + Guid.NewGuid().ToString("N"),
    Scope = "openid profile rn.api.write rn.match.write rn.chat.write",
}));
app.MapPost("/Auth/cachedlogin/forplatformids", () => Results.Json(new TokenResponse
{
    AccessToken = TokenService.IssueAccessToken(Acct, User), TokenType = "Bearer", ExpiresIn = 3600,
}));
app.MapGet("/Auth/role/developer", () => Results.Json(true));

// === Version & config ===
// Real path = /api/versioncheck/islandedversions (from RestoRoom's dnSpy RE of the client). Keep variants too, all answer "ValidForPlay".
foreach (var p in new[] { "/api/versioncheck/islandedversions", "/api/versioncheck/v1", "/api/versioncheck/v2", "/api/versioncheck/v3", "/api/versioncheck/v4", "/api/versioncheck" })
    app.MapGet(p, () => Results.Json(new VersionCheck()));
app.MapGet("/api/config/v2", () => Results.Json(new[]
{
    new ConfigEntry { Key = "Screens.ForceVerification", Value = "0" },
    new ConfigEntry { Key = "Screens.ForceWaitlist", Value = "false" },
    new ConfigEntry { Key = "Maintenance.Enabled", Value = "false" },
    new ConfigEntry { Key = "Photon.Enabled", Value = "true" },
})); // NOTE: real body is ~183 KB of feature flags — extend as we learn required keys
app.MapGet("/api/gameconfigs/v1/all", () => Results.Json(Array.Empty<GameConfig>()));

// === Accounts & session ===
app.MapGet("/Accounts/account/me", () => Results.Json(Me()));
app.MapGet("/Accounts/account/bulk", () => Results.Json(new[] { (Account)Me() }));
app.MapGet("/Accounts/account/{id:long}", (long id) => Results.Json(new Account { AccountId = id, Username = "Player" + id, DisplayName = "Player " + id }));
app.MapGet("/Accounts/parentalcontrol/me", () => Results.Json(new { }));

// === Matchmaking === (endpoint paths from RestoRoom's RE of the client)
app.MapGet("/Matchmaking/player", () => Results.Json(Mp()));
app.MapPost("/Matchmaking/player/login", () => Results.Json(Mp()));
app.MapPost("/Matchmaking/player/logout", () => Results.Json(new { }));
app.MapPost("/Matchmaking/player/heartbeat", () => Results.Json(new { }));
app.MapGet("/Matchmaking/player/qos", () => Results.Json(new { regions = new[] { new { region = "us", ip = "127.0.0.1", port = 5055 } } }));
// connection-info + matchmake drive the Photon connection (response shapes still unknown — minimal localhost stubs for now).
app.MapGet("/Matchmaking/player/connection-info", () => Results.Json(new { host = "127.0.0.1", port = 5055, region = "us" }));
app.MapPut("/Matchmaking/player/gameserverregionpings", () => Results.Json(new { }));
app.MapPost("/Matchmaking/matchmake/dorm", () => Results.Json(new { roomId = 1, instanceId = "dorm-local", host = "127.0.0.1", port = 5055 }));
app.MapPost("/Matchmaking/matchmake/v2/room/{id}", (string id) => Results.Json(new { roomId = id, instanceId = "room-" + id, host = "127.0.0.1", port = 5055 }));
app.MapPost("/Matchmaking/matchmake/none", () => Results.Json(new { }));

// === Rooms & Photon ===
app.MapGet("/Room_server/photon_access_token", () => Results.Json(new PhotonAccessToken { Token = TokenService.IssueAccessToken(Acct, User) }));
app.MapGet("/Room_server/featuredrooms/current", () => Results.Json(featured));
app.MapGet("/Room_server/dormroom/me", () => Results.Json(dorm));
app.MapGet("/Room_server/rooms/hot", () => Results.Json(featured));
app.MapGet("/Room_server/rooms", () => Results.Json(allRooms));
app.MapGet("/Room_server/rooms/search", () => Results.Json(allRooms));
app.MapGet("/Room_server/rooms/bulk", () => Results.Json(allRooms));
app.MapGet("/Room_server/rooms/{id:long}", (long id) => Results.Json(RoomById(id)));

// === Social (relationships / messages) ===
app.MapGet("/api/relationships/v2/get", () => Results.Json(Array.Empty<object>()));
app.MapPost("/api/relationships/v1/ignore", () => Results.Json(new { }));
app.MapGet("/api/messages/v2/get", () => Results.Json(Array.Empty<object>()));

// === Extended RecNet API surface =====================================================================
// Type-correct responses for the rest of the documented endpoints (lists -> [], resources/settings -> {},
// capability checks -> true). Exact bodies await a client dump; shapes are what the client expects.
void GArr(params string[] ps) { foreach (var p in ps) app.MapGet(p, () => Results.Json(Array.Empty<object>())); }
void GObj(params string[] ps) { foreach (var p in ps) app.MapGet(p, () => Results.Json(new { })); }
void GTrue(params string[] ps) { foreach (var p in ps) app.MapGet(p, () => Results.Json(true)); }
void POk(params string[] ps) { foreach (var p in ps) app.MapPost(p, () => Results.Json(new { })); }

// Auth / device / session extras
GObj("/AuthorizeDevice/v1");
POk("/Matchmaking/player/exclusivelogin");
app.MapPut("/Matchmaking/player/statusvisibility", () => Results.Json(new { }));

// Matchmaking & rooms extras
POk("/Matchmaking/roominstance/{id}/reportjoinresult", "/api/rooms/v1/verifyRole", "/api/rooms/v3/report", "/api/quickPlay/v1/getandclear");
GArr("/Matchmaking/rooms/requiring/developer", "/Matchmaking/rooms/requiring/rrplus",
     "/Room_server/rooms/autocomplete_search", "/Room_server/rooms/ownedby/me", "/Room_server/rooms/visitedby/me", "/api/rooms/v1/filters");
GObj("/Room_server/publishState/configs");
app.MapGet("/Room_server/rooms/{id:long}/experience", (long id) => Results.Json(new { }));
app.MapGet("/Room_server/rooms/{id:long}/experience/player", (long id) => Results.Json(new { }));
app.MapGet("/Room_server/rooms/{id:long}/interactionby/me", (long id) => Results.Json(new { }));

// Social extras
POk("/api/relationships/v1/unignore", "/api/relationships/v1/mute", "/api/relationships/v1/unmute");
GArr("/api/messages/v1/friendOnlineStatus");
GObj("/chat/thread", "/chat/thread/party", "/chat/thread/chatPrivacySetting");

// Players & progression
GObj("/api/players/v1/playerPhotoTaggingSetting", "/api/checklist/v1/current", "/api/checklist/v1/complete");
GArr("/api/players/v2/progression/bulk", "/api/progressionEvents/active", "/api/playerReputation/v2/bulk",
     "/api/objectives/v1/current", "/api/gamerewards/v1/pending");
POk("/api/gamerewards/v1/request");

// Avatar & outfits
GArr("/api/customAvatarItems/v1/bulk", "/api/avatar/v1/defaultunlocked", "/api/avatar/v4/items", "/outfits/me/saved");
GTrue("/api/customAvatarItems/v1/isRenderingEnabled", "/api/customAvatarItems/v1/isCreationEnabled", "/api/customAvatarItems/v1/isCreationAllowedForAccount");
GObj("/api/avatar/v2/gifts/generate", "/api/avatar/v3/gifts/generate");

// Economy & commerce
GArr("/econ/roomInventory/player", "/Commerce/api/catalog/v1/all", "/Commerce/api/purchasecampaign/allcurrent/v2",
     "/api/roomEarningsDistributions", "/api/consumables/v1/all", "/api/consumables/v1/query/bulk",
     "/api/roomkeys/v1", "/api/roomkeys/v1/mine");
GObj("/econ/roomOffer/purchaseCounts", "/api/subscriptionseasons/v1/seasons/current");
POk("/api/roomCurrencies/v2/purchase", "/api/consumables/v1/consume", "/api/roomkeys/v1/award", "/api/roomkeys/v1/create");
app.MapGet("/econ/roomInventory/room/{id:long}", (long id) => Results.Json(Array.Empty<object>()));
app.MapGet("/econ/roomInventoryItemTags/room/{id:long}", (long id) => Results.Json(Array.Empty<object>()));
app.MapGet("/econ/roomOffer/room/{id:long}", (long id) => Results.Json(Array.Empty<object>()));
app.MapGet("/econ/roomEconConfig/{id:long}", (long id) => Results.Json(new { }));
app.MapGet("/api/storefronts/v4/balance/{currency}", (string currency) => Results.Json(new { currency, amount = 0 }));
app.MapGet("/api/storefronts/v3/giftdropstore/{id:long}", (long id) => Results.Json(new { }));

// Inventions / images / media
GArr("/api/inventions/v2/mine", "/api/inventions/v1/room", "/api/images/v2/named", "/api/images/v5/cheered/bulk");
POk("/api/images/v1/cheer", "/api/PlayerCheer/v1/create");

// Moderation
GArr("/api/PlayerReporting/v1/voteToKickReasons");
GObj("/api/PlayerReporting/v1/moderationBlockDetails");
GTrue("/api/sanitize/v1/isPure");
POk("/api/PlayerReporting/v1/roomModKick", "/api/sanitize/v1");

// Notifications & clubs
GObj("/Notifications/hub/v1", "/crm/me/config/v3", "/clubs/club/home/me");
POk("/Notifications/hub/v1/negotiate");
GArr("/clubs/announcements/v2/mine/unread", "/clubs/club/mine/member");

// Config & misc
GObj("/api/keepsakes/globalconfig", "/api/communityboard/v2/current");
GArr("/api/keepsakes/categories", "/api/referee/files");

// === Catch-all: always return valid JSON so the client never sees a hard error ===
app.MapFallback(() => Results.Json(new { }));

Console.WriteLine($"=== CreateRoom server ===  HTTP :{httpPort}" + (https ? $"  HTTPS :{httpsPort}" : "  (no HTTPS cert)"));
Console.WriteLine("[capture] logging requests + TLS ClientHellos to: " + logPath);
app.Run();
