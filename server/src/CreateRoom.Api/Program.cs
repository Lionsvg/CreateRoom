using CreateRoom.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Log every incoming request — invaluable for learning what the client asks for.
app.Use(async (ctx, next) =>
{
    Console.WriteLine($"{DateTime.UtcNow:O}  {ctx.Request.Method,-5} {ctx.Request.Host}{ctx.Request.Path}{ctx.Request.QueryString}");
    await next();
});

// --- Auth: OAuth token endpoint (the real one is IdentityServer; this is a stub) ---
app.MapPost("/Auth/connect/token", () => Results.Json(new TokenResponse
{
    AccessToken = "local.dev.access." + Guid.NewGuid().ToString("N"),
    TokenType = "Bearer",
    ExpiresIn = 3600,
    RefreshToken = "local.dev.refresh",
    Scope = "rn.api rn.match rn.chat",
}));

// --- Global config: GET /api/config/v2 (sample; drop in the real captured body here) ---
app.MapGet("/api/config/v2", () => Results.Json(new[]
{
    new ConfigEntry { Key = "Screens.ForceVerification", Value = "1" },
    new ConfigEntry { Key = "Screens.ForceWaitlist", Value = "false" },
    new ConfigEntry { Key = "Door.Featured.Title", Value = "Featured" },
}));

// --- Current account: GET /Accounts/account/me (placeholder) ---
app.MapGet("/Accounts/account/me", () => Results.Json(new Account
{
    AccountId = 1,
    Username = "LocalPlayer",
    DisplayName = "Local Player",
}));

// --- Catch-all: any other route returns {} so the client always gets a valid JSON reply ---
app.MapFallback(() => Results.Json(new { }));

app.Run();
