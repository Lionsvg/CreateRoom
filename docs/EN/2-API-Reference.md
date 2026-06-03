# 2 — Rec Room API Reference

Documentation of the modern Rec Room (RecNet) HTTP protocol, to help reimplement a compatible server.
Compiled from public reverse‑engineering of the modern client and community findings.

> Paths/methods are *inferred* and not all verified against live traffic (the official servers are offline).
> Treat as a working map.

## Base & transport

- **Base URL:** `https://api.rec.net`
- **Transport:** TLS 1.2; the client uses **DNS‑over‑HTTPS** (via `ns.rec.net`) and **certificate pinning**.
- **Auth:** OAuth 2.0 Bearer tokens (RS256 JWT) from `/Auth/connect/token`, validated against a JWKS from the
  OIDC discovery document. Issuer: `https://auth.rec.net`.

## rec.net subdomains (24)

A complete reimplementation must cover these hosts:

| Category | Subdomains |
|---|---|
| Core | `auth`, `accounts`, `api`, `apim` |
| Social / gameplay | `rooms`, `match`, `chat`, `lists`, `clubs`, `discovery` |
| Economy | `econ`, `commerce` |
| UX | `playersettings`, `notify`, `platformnotifications`, `cards`, `leaderboard` |
| Content / assets | `img`, `cdn`, `studiocdn`, `strings-cdn` |
| Other | `ai`, `datacollection`, `cms`, `devportal`, `forum`, `www` |

## Endpoints

### Version & config
| Method | Path | Purpose |
|---|---|---|
| GET | `/api/versioncheck/islandedversions` | Version check |
| GET | `/api/config/v2` | App config / feature flags |
| GET | `/api/gameconfigs/v1/all` | Game configs |

### Auth
| Method | Path | Purpose |
|---|---|---|
| POST | `/Auth/connect/token` | OAuth token login |
| POST | `/Auth/cachedlogin/forplatformids` | Cached platform login |
| GET | `/Auth/role/developer` | Developer‑role check |
| GET | `/.well-known/openid-configuration` · `/Auth/.well-known/openid-configuration/jwks` | OIDC discovery + JWKS |

### Accounts & session
| Method | Path | Purpose |
|---|---|---|
| GET | `/Accounts/account/me` · `/account/bulk` · `/account/{id}` | Account data |
| GET | `/Accounts/parentalcontrol/me` | Parental‑control settings |

### Matchmaking
| Method | Path | Purpose |
|---|---|---|
| GET | `/Matchmaking/player` · `/player/connection-info` | Player state / connection info |
| POST | `/Matchmaking/player/login` · `/logout` · `/heartbeat` | Session lifecycle |
| PUT | `/Matchmaking/player/gameserverregionpings` | Region pings |
| POST | `/Matchmaking/matchmake/dorm` · `/matchmake/v2/room/{id}` · `/matchmake/none` | Matchmaking |

### Rooms & Photon
| Method | Path | Purpose |
|---|---|---|
| GET | `/Room_server/rooms` · `/rooms/{id}` · `/rooms/bulk` · `/rooms/search` | Room data |
| GET | `/Room_server/featuredrooms/current` · `/dormroom/me` | Featured rooms / your dorm |
| GET | `/Room_server/photon_access_token` | Token to connect to Photon realtime |

### Social
| Method | Path | Purpose |
|---|---|---|
| GET | `/api/relationships/v2/get` · `/api/messages/v2/get` | Relationships / messages |
| POST | `/api/relationships/v1/ignore` | Ignore a player |

## Version‑check response shape

To reimplement the version‑check endpoint, the response is a JSON object (PascalCase):

```jsonc
{
  "VersionStatus": 0,             // enum: 0 = ValidForPlay, 1 = UpdateRequired
  "UpdateNotificationStage": 0,   // enum: 0 = None, 1 = Silent, 2 = Warn, 3 = Prompt, 4 = Require
  "IsCrossPlayDisabled": false,
  "RequiresUpdate": false
}
```

A server that wants to report "ready to play" returns `VersionStatus = 0` and `UpdateNotificationStage = 0`.

➡️ Continue with [3 — Architecture & Status](3-Architecture-and-Status.md).
