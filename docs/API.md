# Rec Room — Recovered API reference

> Compiled from hands-on analysis of the live **modern** client (process memory + on-disk HTTP cache)
> and cross-referenced with community findings. Base URL: **`https://api.rec.net`**.
> All traffic is **TLS 1.2/1.3**, auth is **OAuth** (`/Auth/connect/token`), realtime is **Photon**.
> ⚠️ Endpoint paths are known; request/response **bodies are mostly still unknown** (client is pinned).

## Hosts (`*.rec.net`)
| Host | Likely purpose |
|---|---|
| `api` | general / catch-all API |
| `auth` | authentication, OAuth tokens |
| `accounts` | account / profile data |
| `match` | matchmaking |
| `rooms` | room listings / room data |
| `ns` / `ns-fd` | namespace / front-door routing |
| `apim` | API gateway |
| `chat` | in-game chat |
| `lists` | friend / block lists |
| `clubs` | clubs / groups |
| `econ` / `commerce` | economy, currency, shop |
| `cards` | profile cards |
| `discovery` | room discovery / browse |
| `playersettings` | player preferences |
| `notify` / `platformnotifications` | notifications |
| `leaderboard` | leaderboards |
| `ai` | Roomie AI |
| `cdn` / `img` / `studiocdn` / `strings-cdn` | asset / image / localization CDNs (Azure) |
| `config` / `data` / `pref` / `bugreporting` / `gamelogs` | config / telemetry / logs |
| `studio` / `www` / `forum` / `devportal` | studio, website, forum, dev portal |

## Endpoints by category

### Authentication
- `POST /Auth/connect/token` — OAuth token (login)
- `POST /Auth/cachedlogin/forplatformids` — cached platform login
- `GET  /Auth/role/developer` — developer role check
- `/AuthorizeDevice/v1/` — device authorization

### Accounts & session
- `GET  /Accounts/account/me` · `/account/bulk` · `/parentalcontrol/me`
- `GET  /Matchmaking/player` · `POST /player/login` · `/logout` · `/heartbeat`
- `GET  /Matchmaking/player/qos` · `/connection-info` · `POST /exclusivelogin`
- `PUT  /Matchmaking/player/gameserverregionpings` · `/statusvisibility`

### Matchmaking & rooms
- `POST /Matchmaking/matchmake/dorm` · `/v2/room/{id}` · `/none`
- `POST /Matchmaking/roominstance/{id}/reportjoinresult`
- `GET  /Matchmaking/rooms/requiring/developer` · `/rrplus`
- `GET  /Room_server/rooms` · `/{id}` · `/bulk` · `/hot` · `/search` · `/autocomplete_search`
- `GET  /Room_server/dormroom/me` · `/featuredrooms/current` · `/rooms/ownedby/me` · `/visitedby/me`
- `GET  /Room_server/rooms/{id}/experience` · `/experience/player` · `/interactionby/me`
- `GET  /Room_server/rooms/{id}/subrooms/{sub}/saves/{save}`
- `GET  /Room_server/photon_access_token` — **Photon networking token**
- `GET  /Room_server/publishState/configs`
- `GET  /api/rooms/v1/filters` · `POST /v1/verifyRole` · `/v3/report`
- `POST /api/quickPlay/v1/getandclear`

### Social
- `GET  /api/relationships/v2/get` · `POST /v1/{ignore,unignore,mute,unmute}`
- `GET  /api/messages/v1/friendOnlineStatus` · `/v2/get`
- `GET  /chat/thread` · `/thread/party` · `/thread/chatPrivacySetting`

### Players & progression
- `GET  /api/players/v1/playerPhotoTaggingSetting` · `/v2/progression/bulk?id=`
- `GET  /api/progressionEvents/active` · `/event/id`
- `GET  /api/playerReputation/v2/bulk?id=`
- `GET  /api/objectives/v1/*` · `/api/checklist/v1/{current,complete}` · `/api/gamerewards/v1/{pending,request}`

### Avatar & outfits
- `GET  /api/customAvatarItems/v1/bulk` · `/isRenderingEnabled` · `/isCreationEnabled` · `/isCreationAllowedForAccount`
- `GET  /api/avatar/v1/defaultunlocked` · `/v2/gifts/generate` · `/v3/gifts/generate` · `/v4/items`
- `GET  /outfits/me/saved`

### Economy & commerce
- `GET  /econ/roomInventory/room/{id}` · `/player` · `/roomInventoryItemTags/room/{id}`
- `GET  /econ/roomOffer/room/{id}` · `/purchaseCounts` · `/roomEconConfig/{id}`
- `GET  /Commerce/api/catalog/v1/all` · `/purchasecampaign/allcurrent/v2`
- `GET  /api/storefronts/v4/balance/{currency}` · `/v3/giftdropstore/{id}`
- `POST /api/roomCurrencies/v2/purchase` · `GET /api/roomEarningsDistributions`
- `GET  /api/consumables/v1/all` · `/consume` · `/query/bulk`
- `GET  /api/roomkeys/v1/` · `/award` · `/create` · `/mine`
- `GET  /api/subscriptionseasons/v1/seasons/current`

### Inventions
- `GET  /api/inventions/v2/mine` · `/v1/room?id=`

### Images & media
- `GET  /api/images/v2/named` · `/v5/cheered/bulk` · `POST /v1/cheer` · `/PlayerCheer/v1/create`

### Moderation
- `GET  /api/PlayerReporting/v1/voteToKickReasons` · `/moderationBlockDetails` · `POST /roomModKick`
- `POST /api/sanitize/v1` · `GET /api/sanitize/v1/isPure`

### Notifications & clubs
- `GET  /Notifications/hub/v1` · `POST /hub/v1/negotiate` · `GET /crm/me/config/v3`
- `GET  /clubs/announcements/v2/mine/unread` · `/club/home/me` · `/club/mine/member`

### Config & misc
- `GET  /api/config/v2` — **real 183 KB body captured** (key/value feature flags)
- `GET  /api/gameconfigs/v1/all` · `/api/versioncheck/v4?v=…` · `/islandedversions`
- `GET  /api/keepsakes/globalconfig` · `/categories` · `/api/communityboard/v2/current`
- `GET  /api/referee/files` — Referee (server-authority) files
- `roomieai/*` — Roomie AI (OpenAI Realtime over WebSocket)

## Notes
- **Realtime**: client gets a token from `/Room_server/photon_access_token`, then connects to **Photon**.
- **Auth**: IdentityServer-style OAuth at `/Auth/connect/token`.
- **Local config**: client references `config/certs/selfsigned_2021.crt`, `config/configset_%s.vdf`, `config/preferences_%s.vdf`.
- **Third-party**: OpenAI Realtime (Roomie), Amplitude, Backtrace, Unity UCA, Steam, Apple.
