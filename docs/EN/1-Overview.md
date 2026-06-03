# 1 — Overview

## The goal

Rec Room (the **modern** 2026 client) shut down its official servers on **2026‑06‑01**. This project
documents the modern client's network protocol and explores reviving it on a private server, **"CreateRoom"**.

This targets the **latest/modern** client — not the "old‑build" (2021‑era) revivals such as RebornRec,
EpicQuest, OpenRec or LunarRec.

## What the player sees today

Launching the retail client now reaches `api.rec.net` (still alive behind Cloudflare), receives a
**version‑check** response that says **"update required"**, and shows:

> *"There's a new version of Rec Room available. You'll need to update before you can play."*

…then the game exits. This is a **server‑side shutdown flag**: there is no newer version to install, so
re‑installing does not help. It's an intentional "everyone off" signal for the shutdown.

## How the client is built (high level)

- A native **launcher** (`Recroom_Release.exe`) starts first and launches the actual game (`RecRoom.exe`).
- The game is **Unity / IL2CPP**. Its HTTP stack is **BestHTTP** (with a bundled **BouncyCastle** TLS
  library); realtime multiplayer is **Photon**.
- The client resolves hostnames via **DNS‑over‑HTTPS** and uses **TLS certificate pinning**.
- An **anti‑cheat** component runs alongside the game.

`api.rec.net` remains reachable via Cloudflare, which is why the retail client still receives the shutdown
"update required" answer.

## Status

| Piece | Status |
|---|---|
| Documenting the API / protocol | ✅ Done (see [2 — API Reference](2-API-Reference.md)) |
| **Server reimplementation** (answers the version check, etc.) | ✅ Works server‑side |
| Pointing the **retail client** at a private server | ⛔ Open problem — blocked by cert pinning + anti‑cheat (team‑scale, out of scope here) |
| Actually playing | ⛔ Requires the above + auth + rooms + Photon |

**Bottom line:** the protocol is documented and a server can answer it. Getting the **retail** client to use a
private server is the hard, unsolved part — see [3 — Architecture & Status](3-Architecture-and-Status.md).

➡️ Continue with [2 — API Reference](2-API-Reference.md).
