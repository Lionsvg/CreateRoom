# CreateRoom — Architecture (C# / .NET)

**Why C#/.NET:** the captured API response carried `x-powered-by: ASP.NET`, the auth endpoint is
`/Auth/connect/token` (IdentityServer pattern), Photon Server is .NET, and the client's DTOs are C#.
Reimplementing in ASP.NET Core lets us match the original behavior closely instead of guessing.

## Solution layout
Start as a **modular monolith** (one ASP.NET Core app, one controller area per domain), split into
separate services later if needed.

```
CreateRoom.sln
├── src/
│   ├── CreateRoom.Auth         # Duende IdentityServer  -> /Auth/connect/token, /Auth/role/*
│   ├── CreateRoom.Api          # ASP.NET Core Web API   -> accounts, config, rooms, players, econ...
│   ├── CreateRoom.Matchmaking  # /Matchmaking/*, /Room_server/*  (+ issues Photon tokens)
│   ├── CreateRoom.Realtime     # Photon Server plugin / relay (later phase)
│   ├── CreateRoom.Data         # EF Core DbContext + entities (SQLite to start)
│   └── CreateRoom.Models       # shared C# DTOs (request/response types)
└── docs/
```

## Service responsibilities

### CreateRoom.Auth (priority #1 — nothing works without login)
- **Duende IdentityServer** (free for non‑commercial / small projects).
- Implements `POST /Auth/connect/token` (OAuth) so the client receives a token it accepts.
- `POST /Auth/cachedlogin/forplatformids`, `GET /Auth/role/developer`.
- Open by default (anyone can log in) since there's no real account backend.

### CreateRoom.Api (priority #2)
- `GET /api/config/v2` → serve the **captured real config body** (already recovered, 183 KB).
- `GET /Accounts/account/me`, `/bulk`, `/parentalcontrol/me`.
- Players, images, avatar, inventions, economy, clubs, notifications, moderation (stub → real).
- Controllers map 1:1 to the route groups in `docs/API.md`.

### CreateRoom.Matchmaking (priority #3)
- `/Matchmaking/player/login|logout|heartbeat|qos|connection-info`.
- `/Room_server/rooms*`, room listings/search/featured.
- `GET /Room_server/photon_access_token` → mint a Photon token for the realtime layer.

### CreateRoom.Realtime (hardest — later)
- Photon Server (Windows/.NET) self‑hosted, or a Photon‑protocol relay.
- The client connects here after getting the token from Matchmaking.

### CreateRoom.Data
- EF Core, **SQLite** to start (zero setup), Postgres later.
- Entities: Account, Room, RoomInstance, Inventory, Outfit, Subscription, etc.

## Build order (roadmap)
1. **Auth** returns a valid token → client gets past login.
2. **`/api/config/v2`** returns the real captured config → client loads its settings.
3. **Accounts + Matchmaking** → client reaches the home screen / can query rooms.
4. **Photon** → actually entering rooms (realtime).
5. Economy, inventory, social, UGC.

## The client‑side prerequisite
Connecting the **retail modern client** to a private server is a separate, unsolved problem — its protections
prevent simply redirecting it (see [Architecture & Status](EN/3-Architecture-and-Status.md)). The server can
be **built and tested independently** (via curl / a test client) in the meantime.

## Getting started (for contributors)
```bash
dotnet new sln -n CreateRoom
dotnet new webapi -n CreateRoom.Api   -o src/CreateRoom.Api
dotnet new classlib -n CreateRoom.Models -o src/CreateRoom.Models
# add Duende IdentityServer for CreateRoom.Auth, EF Core for CreateRoom.Data, etc.
dotnet sln add src/**/*.csproj
dotnet run --project src/CreateRoom.Api
```
Requires the **.NET SDK** (8 or 9). HTTPS dev cert: `dotnet dev-certs https --trust`.
