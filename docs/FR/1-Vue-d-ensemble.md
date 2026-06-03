# 1 — Vue d'ensemble

## L'objectif

Rec Room (le client **moderne** de 2026) a fermé ses serveurs officiels le **01/06/2026**. Ce projet
documente le protocole réseau du client moderne et explore sa renaissance sur un serveur privé,
**« CreateRoom »**.

On vise le client **le plus récent / moderne** — pas les revivals « ancien build » (époque 2021) comme
RebornRec, EpicQuest, OpenRec ou LunarRec.

## Ce que voit le joueur aujourd'hui

Lancer le client commercial maintenant joint `api.rec.net` (toujours en vie derrière Cloudflare), reçoit une
réponse de **version-check** disant **« update required »**, et affiche :

> *« There's a new version of Rec Room available. You'll need to update before you can play. »*

…puis le jeu se ferme. C'est un **drapeau de fermeture côté serveur** : il n'existe pas de version plus
récente à installer, donc réinstaller ne sert à rien. C'est un signal « tout le monde dehors » volontaire.

## Comment le client est construit (haut niveau)

- Un **lanceur** natif (`Recroom_Release.exe`) démarre d'abord et lance le vrai jeu (`RecRoom.exe`).
- Le jeu est en **Unity / IL2CPP**. Sa stack HTTP est **BestHTTP** (avec une lib **BouncyCastle** embarquée) ;
  le temps réel multijoueur est **Photon**.
- Le client résout les noms via **DNS-over-HTTPS** et utilise le **pinning de certificat TLS**.
- Un composant **anti-triche** tourne avec le jeu.

`api.rec.net` reste joignable via Cloudflare, c'est pourquoi le client commercial reçoit encore la réponse de
fermeture « update required ».

## État

| Pièce | État |
|---|---|
| Documenter l'API / le protocole | ✅ Fait (voir [2 — Référence API](2-Reference-API.md)) |
| **Réimplémentation serveur** (répond au version-check, etc.) | ✅ Fonctionne côté serveur |
| Pointer le **client commercial** vers un serveur privé | ⛔ Problème ouvert — bloqué par pinning + anti-triche (échelle équipe, hors périmètre ici) |
| Jouer réellement | ⛔ Demande ce qui précède + auth + rooms + Photon |

**En résumé :** le protocole est documenté et un serveur peut y répondre. Faire utiliser un serveur privé au
client **commercial** est la partie difficile et non résolue — voir [3 — Architecture & État](3-Architecture-et-Etat.md).

➡️ Suite : [2 — Référence API](2-Reference-API.md).
