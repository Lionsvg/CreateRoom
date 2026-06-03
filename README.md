# CreateRoom

Community documentation & notes toward reviving the **modern** Rec Room client on a private server, after the
official **2026‑06‑01** shutdown.

Documentation communautaire & notes pour faire revivre le client **moderne** de Rec Room sur un serveur privé,
après la fermeture officielle du **01/06/2026**.

> This repository contains **documentation and protocol notes only** — no game files, binaries, dumps, keys,
> or copyrighted assets. It documents the public network protocol (like other revival projects) and an honest
> status of the effort. / Ce dépôt contient **uniquement de la documentation et des notes de protocole** — pas
> de fichiers de jeu, binaires, dumps, clés ou assets sous copyright.

---

## 📚 Contents / Sommaire

| 🇬🇧 English | 🇫🇷 Français | |
|---|---|---|
| [1 — Overview](docs/EN/1-Overview.md) | [1 — Vue d'ensemble](docs/FR/1-Vue-d-ensemble.md) | Goal, shutdown, how the client is built, status |
| [2 — API Reference](docs/EN/2-API-Reference.md) | [2 — Référence API](docs/FR/2-Reference-API.md) | rec.net endpoints, subdomains, the version‑check schema |
| [3 — Architecture & Status](docs/EN/3-Architecture-and-Status.md) | [3 — Architecture & État](docs/FR/3-Architecture-et-Etat.md) | How it's protected, what's done, what's open |

## ⚡ Status

- ✅ **Server side reimplemented** — a private server that speaks the Rec Room (RecNet) protocol and answers
  the version check (the "ready to play" response).
- ⛔ **Connecting the retail client is the open problem** — the modern client is protected by certificate
  pinning and an anti‑cheat that prevent simply pointing it at a private server. This is a **team‑scale**
  problem and is **out of scope** for this documentation repo.

## 💬 Community / Communauté

- **CreateRoom** — join the Discord / rejoins le Discord : <https://discord.gg/kVXNAqhmPf>

## ⚠️ Scope & legality / Cadre & légalité

This is **personal research / documentation** about software interoperability and a defunct online service.
It intentionally **omits any step‑by‑step instructions to circumvent anti‑cheat or certificate pinning**, and
**redistributes no game code or assets**. / Recherche / documentation **personnelle** sur l'interopérabilité
et un service en ligne disparu. Aucune instruction de contournement, aucun code/asset du jeu.
