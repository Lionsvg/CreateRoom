# 2 — Référence de l'API Rec Room

Documentation du protocole HTTP du Rec Room moderne (RecNet), pour aider à réimplémenter un serveur
compatible. Compilé depuis du reverse-engineering public du client moderne et des trouvailles communautaires.

> Les chemins/méthodes sont *déduits* et pas tous vérifiés sur du trafic réel (les serveurs officiels sont
> hors ligne). À considérer comme une carte de travail.

## Base & transport

- **URL de base :** `https://api.rec.net`
- **Transport :** TLS 1.2 ; le client utilise le **DNS-over-HTTPS** (via `ns.rec.net`) et le **pinning de
  certificat**.
- **Auth :** jetons OAuth 2.0 Bearer (JWT RS256) depuis `/Auth/connect/token`, validés contre un JWKS issu du
  document de découverte OIDC. Émetteur : `https://auth.rec.net`.

## Sous-domaines rec.net (24)

Une réimplémentation complète doit couvrir ces hôtes :

| Catégorie | Sous-domaines |
|---|---|
| Cœur | `auth`, `accounts`, `api`, `apim` |
| Social / jeu | `rooms`, `match`, `chat`, `lists`, `clubs`, `discovery` |
| Économie | `econ`, `commerce` |
| UX | `playersettings`, `notify`, `platformnotifications`, `cards`, `leaderboard` |
| Contenu / assets | `img`, `cdn`, `studiocdn`, `strings-cdn` |
| Autres | `ai`, `datacollection`, `cms`, `devportal`, `forum`, `www` |

## Endpoints

### Version & config
| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/api/versioncheck/islandedversions` | Version-check |
| GET | `/api/config/v2` | Config / feature-flags |
| GET | `/api/gameconfigs/v1/all` | Configs de jeu |

### Auth
| Méthode | Chemin | Rôle |
|---|---|---|
| POST | `/Auth/connect/token` | Login OAuth (jeton) |
| POST | `/Auth/cachedlogin/forplatformids` | Login plateforme en cache |
| GET | `/Auth/role/developer` | Vérif du rôle développeur |
| GET | `/.well-known/openid-configuration` · `/Auth/.well-known/openid-configuration/jwks` | Découverte OIDC + JWKS |

### Comptes & session
| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/Accounts/account/me` · `/account/bulk` · `/account/{id}` | Données de compte |
| GET | `/Accounts/parentalcontrol/me` | Contrôle parental |

### Matchmaking
| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/Matchmaking/player` · `/player/connection-info` | État joueur / infos de connexion |
| POST | `/Matchmaking/player/login` · `/logout` · `/heartbeat` | Cycle de session |
| PUT | `/Matchmaking/player/gameserverregionpings` | Pings de région |
| POST | `/Matchmaking/matchmake/dorm` · `/matchmake/v2/room/{id}` · `/matchmake/none` | Matchmaking |

### Rooms & Photon
| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/Room_server/rooms` · `/rooms/{id}` · `/rooms/bulk` · `/rooms/search` | Données des rooms |
| GET | `/Room_server/featuredrooms/current` · `/dormroom/me` | Rooms en vedette / ton dortoir |
| GET | `/Room_server/photon_access_token` | Jeton pour se connecter au temps réel Photon |

### Social
| Méthode | Chemin | Rôle |
|---|---|---|
| GET | `/api/relationships/v2/get` · `/api/messages/v2/get` | Relations / messages |
| POST | `/api/relationships/v1/ignore` | Ignorer un joueur |

## Forme de la réponse version-check

Pour réimplémenter l'endpoint de version-check, la réponse est un objet JSON (PascalCase) :

```jsonc
{
  "VersionStatus": 0,             // enum : 0 = ValidForPlay, 1 = UpdateRequired
  "UpdateNotificationStage": 0,   // enum : 0 = None, 1 = Silent, 2 = Warn, 3 = Prompt, 4 = Require
  "IsCrossPlayDisabled": false,
  "RequiresUpdate": false
}
```

Un serveur qui veut signaler « prêt à jouer » renvoie `VersionStatus = 0` et `UpdateNotificationStage = 0`.

➡️ Suite : [3 — Architecture & État](3-Architecture-et-Etat.md).
