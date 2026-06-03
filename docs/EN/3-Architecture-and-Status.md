# 3 — Architecture & Status

A high‑level, honest summary of how the modern client is protected and where a private‑server revival stands.
**This document deliberately omits any circumvention instructions.**

## How the modern client is protected

The retail client is hardened against being pointed at a third‑party server:

- **Certificate pinning** — the client validates server certificates against its own pinned material and
  **ignores the OS trust store**, so a self‑signed certificate for `*.rec.net` is rejected.
- **DNS‑over‑HTTPS** — hostnames are resolved over an encrypted channel rather than plain DNS.
- **Anti‑cheat** — a dedicated anti‑cheat component (commercial, hardened) runs with the game and reacts to
  tampering with the running process.
- **Native launcher** — a separate, hardened launcher performs startup checks (including a version check)
  before the game runs.
- **IL2CPP** — the game logic is compiled to native code (Unity IL2CPP) with an obfuscating protector,
  making static analysis difficult.

Together these are designed specifically to resist redirecting the client to an unofficial server.

## What works

- The **network protocol is documented** ([2 — API Reference](2-API-Reference.md)): endpoints, subdomains,
  auth (OAuth/JWT/JWKS/OIDC), and the version‑check response shape.
- A **private server** ("CreateRoom") can answer these requests — including a "ready to play" version check —
  using standard web tooling. The server side of a revival is achievable.

## What's open (the wall)

Getting the **retail** client to actually use a private server is unsolved here. In short:

- The protections above (cert pinning + the hardened launcher's own version check + the anti‑cheat) gate the
  client in a way that a single person cannot work around responsibly.
- This is consistent with the broader community, where reliably connecting the modern client to a private
  server remains unsolved.

This documentation therefore focuses on the **protocol and the server side**, and treats connecting the
retail client as a **team‑scale, out‑of‑scope** problem.

## Honest conclusion

- The **server is the achievable, reusable part** — and the protocol map here is exactly what a server needs.
- **Playing the modern client on a private server is a long‑term, collaborative effort**, not something one
  person can finish. The realistic path is a broader, collaborative community effort.

## Community

- Join the **CreateRoom** community on Discord: <https://discord.gg/kVXNAqhmPf>.

---

_This is an interoperability/documentation project about a defunct online service. It contains no game code
or assets and no instructions to bypass technical protections._
