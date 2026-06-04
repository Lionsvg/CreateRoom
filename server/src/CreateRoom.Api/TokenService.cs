using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CreateRoom.Api;

/// <summary>
/// Minimal IdentityServer-style OAuth token issuer: signs RS256 JWTs and publishes its public key
/// via JWKS + OIDC discovery, so the client (which validates tokens against the issuer's JWKS) accepts them.
/// Hand-rolled (no NuGet) using built-in RSA. Dev key is generated once at startup.
/// </summary>
public static class TokenService
{
    public const string Issuer = "https://auth.rec.net";
    public const string Kid = "createroom-dev-1";
    private static readonly RSA Rsa = RSA.Create(2048);

    private static string B64Url(byte[] b) => Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static string B64Url(string s) => B64Url(Encoding.UTF8.GetBytes(s));

    /// <summary>Issue a signed access token (JWT) for an account.</summary>
    public static string IssueAccessToken(long accountId, string username, int lifetimeSeconds = 3600)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var header = new Dictionary<string, object> { ["alg"] = "RS256", ["typ"] = "JWT", ["kid"] = Kid };
        var payload = new Dictionary<string, object>
        {
            ["iss"] = Issuer,
            ["aud"] = "rec.net",
            ["sub"] = accountId.ToString(),
            ["accountId"] = accountId,
            ["username"] = username,
            ["iat"] = now,
            ["exp"] = now + lifetimeSeconds,
            ["scope"] = new[] { "rn.api.write", "rn.match.write", "rn.chat.write" },
        };
        string signingInput = B64Url(JsonSerializer.Serialize(header)) + "." + B64Url(JsonSerializer.Serialize(payload));
        byte[] sig = Rsa.SignData(Encoding.ASCII.GetBytes(signingInput), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return signingInput + "." + B64Url(sig);
    }

    /// <summary>JWKS document (public key) for token validation — served at the jwks_uri.</summary>
    public static object Jwks()
    {
        var p = Rsa.ExportParameters(false);
        return new { keys = new[] { new { kty = "RSA", use = "sig", alg = "RS256", kid = Kid, n = B64Url(p.Modulus!), e = B64Url(p.Exponent!) } } };
    }

    /// <summary>OIDC discovery document — points the client at our endpoints + JWKS.</summary>
    public static object Discovery() => new
    {
        issuer = Issuer,
        authorization_endpoint = Issuer + "/Auth/connect/authorize",
        token_endpoint = Issuer + "/Auth/connect/token",
        jwks_uri = Issuer + "/Auth/.well-known/openid-configuration/jwks",
        userinfo_endpoint = Issuer + "/Auth/connect/userinfo",
        response_types_supported = new[] { "code", "token", "id_token" },
        subject_types_supported = new[] { "public" },
        id_token_signing_alg_values_supported = new[] { "RS256" },
        scopes_supported = new[] { "openid", "profile", "rn.api.write", "rn.match.write", "rn.chat.write" },
        grant_types_supported = new[] { "authorization_code", "client_credentials", "refresh_token", "password" },
    };
}
